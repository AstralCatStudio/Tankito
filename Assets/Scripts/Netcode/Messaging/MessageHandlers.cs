using System;
using UnityEngine;
using Unity.Collections;
using Unity.Netcode;
using Tankito.Utils;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using Tankito.Netcode.Simulation;
using System.Collections.Generic;

namespace Tankito.Netcode.Messaging
{
    public struct MessageName
    {
        public static string ClockSignal => "ClockSignal";
        public static string InputWindow => "InputWindow";
        public static string RelayInputWindow => "RelayWindow";
        public static string SimulationSnapshot => "SimulationSnapshot";
    }

    public class MessageHandlers : MonoBehaviour
    {
        public static MessageHandlers Instance;
        [SerializeField] private bool DEBUG_INPUT = false;
        [SerializeField] private bool DEBUG_CLOCK = false;
        [SerializeField] private bool DEBUG_SNAPSHOTS = false;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }
        }

        /// Se registran los mensajes en el GameManager ///
        /// Se deregistran los mensajes en el Boton Exit ///

        public void SendClockSignal(ClockSignal signal)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Debug.LogException(new InvalidOperationException("Client's can't send clock signals"));
                return;
            }
            
            
            var customMessagingManager = NetworkManager.Singleton.CustomMessagingManager;
            var writer = new FastBufferWriter(FastBufferWriter.GetWriteSize(signal), Allocator.Temp);

            using (writer)
            {
                writer.WriteValue(signal);
                customMessagingManager.SendNamedMessageToAll(MessageName.ClockSignal, writer, NetworkDelivery.ReliableSequenced);
            }

            if (NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient) SimClock.Instance.StartClock();
            if (DEBUG_CLOCK) Debug.Log($"Sent clock signal: {signal}");
        }

        private void SendThrottleSignal(ulong clientId)
        {
            if (!NetworkManager.Singleton.IsServer) return;
            int throttleTicks = ServerSimulationManager.Instance.remoteInputTanks[clientId].IdealBufferSize - ServerSimulationManager.Instance.remoteInputTanks[clientId].BufferSize;
            var throttleSignal = new ClockSignal(ClockSignalHeader.Throttle, throttleTicks, SimClock.TickCounter);
            var throttleWriter = new FastBufferWriter(FastBufferWriter.GetWriteSize(throttleSignal), Allocator.Temp);

            using (throttleWriter)
            {
                throttleWriter.WriteValue(throttleSignal);
                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(MessageName.ClockSignal, clientId, throttleWriter, NetworkDelivery.Unreliable);
            }
        }

        public void ReceiveClockSignal(ulong serverId, FastBufferReader payload)
        {
            if (NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient) return;
            
            if (serverId != NetworkManager.ServerClientId)
            {
                Debug.LogWarning("Can only receive clock signal messages from the server!");
                return;
            }

            ClockSignal signal;
            payload.ReadValue(out signal);

            switch(signal.header)
            {
                case ClockSignalHeader.Start:
                    SimClock.Instance.StartClock();
                    break;

                case ClockSignalHeader.Stop:
                    SimClock.Instance.StopClock();
                    break;

                case ClockSignalHeader.Throttle:
                    SimClock.Instance.ThrottleClock(signal.throttleTicks, signal.serverTime);
                    break;

                default:
                    Debug.LogException(new InvalidOperationException("Invalid clock signal header: " + signal.header));
                    return;
            }

            if (DEBUG_CLOCK) Debug.Log($"[{SimClock.TickCounter}]Received clock signal: {signal}");
        }

        /// <summary>
        /// Invoke to send an <see cref="InputWindowBuffer.inputWindow"/> to the server
        /// </summary>
        public void SendInputWindowToServer(FixedSizeQueue<InputPayload> inputWindow)
        {
            if (!NetworkManager.Singleton.IsClient)
            {
                throw new InvalidOperationException("Server should not send raw inputs to clients, it should relay them through RelayInput messages!");
            }
            
            var writer = new FastBufferWriter(FastBufferWriter.GetWriteSize<InputPayload>()*inputWindow.Count, Allocator.Temp);
            var customMessagingManager = NetworkManager.Singleton.CustomMessagingManager;

            using (writer)
            {
                foreach(var input in inputWindow)
                {
                    writer.WriteValue(input);
                }
                
                customMessagingManager.SendNamedMessage(MessageName.InputWindow, NetworkManager.ServerClientId, writer, NetworkDelivery.Unreliable);
            }

            if (DEBUG_INPUT)
            {
                if (DEBUG_INPUT) Debug.Log($"[{SimClock.TickCounter}]Sent input window: Ticks[{inputWindow.First().timestamp}-{inputWindow.Last().timestamp}]");
            }
        }

        /// <summary>
        /// Invoked when receiving a custom message of type <see cref="MessageName.InputWindow"/>
        /// </summary>
        public void ReceiveInputWindow(ulong senderId, FastBufferReader payload)
        {
            if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                Debug.LogException(new InvalidOperationException($"Clients receiving raw inputs is not supported. Did senderId[{senderId}] mean to relay an input payload?"));
                return;
            }

            List<InputPayload> receivedInputWindow = new List<InputPayload>();

            while (payload.TryBeginRead(FastBufferWriter.GetWriteSize<InputPayload>()))
            {
                InputPayload inputPayload;
                payload.ReadValue(out inputPayload);
                receivedInputWindow.Add(inputPayload);
            }

            // Relay Client inputs
            RelayClientInput(senderId, receivedInputWindow.ToArray());

            if (senderId != NetworkManager.Singleton.LocalClientId)
            {
                // Store inputWindow
                ServerSimulationManager.Instance.remoteInputTanks[senderId].AddInput(receivedInputWindow.ToArray());

                // Respond with throttling signal
                /*int throttleTicks = ServerSimulationManager.Instance.remoteInputTanks[senderId].IdealBufferSize-ServerSimulationManager.Instance.remoteInputTanks[senderId].BufferSize;
                var throttleSignal = new ClockSignal(ClockSignalHeader.Throttle, throttleTicks, SimClock.TickCounter);
                var throttleWriter = new FastBufferWriter(FastBufferWriter.GetWriteSize(throttleSignal), Allocator.Temp);

                using (throttleWriter)
                {
                    throttleWriter.WriteValue(throttleSignal);
                    NetworkManager.CustomMessagingManager.SendNamedMessage(MessageName.ClockSignal, senderId, throttleWriter, NetworkDelivery.Unreliable);
                }*/
            }

            if (DEBUG_INPUT)
            {
                Debug.Log($"[{SimClock.TickCounter}]Recieved input window from client {senderId}: Ticks[{receivedInputWindow.First().timestamp}-{receivedInputWindow.Last().timestamp}]");
            }
        }

        public void RelayClientInput(ulong originalSenderId, InputPayload[] windowToRelay)
        {
            var relayWriter = new FastBufferWriter(sizeof(ulong)+FastBufferWriter.GetWriteSize(windowToRelay), Allocator.Temp);

            using (relayWriter)
            {
                relayWriter.WriteValueSafe(originalSenderId);
                foreach(var input in windowToRelay)
                {
                    relayWriter.WriteValue(input);
                }

                // Exclude originalSender and Server IDs from relay message targets
                var relayTargets = NetworkManager.Singleton.ConnectedClientsIds.Where(id => (id != originalSenderId) && (id != NetworkManager.ServerClientId)).ToArray();
                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(MessageName.RelayInputWindow, relayTargets, relayWriter, NetworkDelivery.Unreliable);

                if (DEBUG_INPUT)
                {
                    var relayTargetsString = "( ";
                    Array.ForEach(relayTargets, t => relayTargetsString += t.ToString() + ((t != relayTargets.Last()) ? ", " : " )"));
                    Debug.Log($"[{SimClock.TickCounter}]Relaying client[{originalSenderId}]'s inputs to: " + relayTargetsString);
                }
            }
        }

        public void ReceiveRelayedInputWindow(ulong senderId, FastBufferReader inputRelayPayload)
        {
            if (senderId != NetworkManager.ServerClientId)
            {
                Debug.LogException(new InvalidOperationException("Relayed Inputs are only taken from the server, clients shouldn't be able to emit them."));
                return;
            }
            if (NetworkManager.Singleton.IsServer)
            {
                Debug.LogException(new InvalidOperationException("Input mustn't be relayed to the server (it already received the input window)."));
                return;
            }

            List<InputPayload> relayedInputWindow = new List<InputPayload>();
            ulong originalSenderId;

            inputRelayPayload.ReadValueSafe(out originalSenderId);

            if (originalSenderId == NetworkManager.Singleton.LocalClientId)
            {
                if (DEBUG_INPUT)
                {
                    Debug.Log($"[{SimClock.TickCounter}]Recieved relayed input window from client {originalSenderId}(SELF) - TODO: Implement input payload acknowledgement.");
                    return;
                }
            }

            while (inputRelayPayload.TryBeginRead(FastBufferWriter.GetWriteSize<InputPayload>()))
            {
                InputPayload inputPayload;
                inputRelayPayload.ReadValue(out inputPayload);
                relayedInputWindow.Add(inputPayload);
            }

            // Store RelayedInputWindow to emulated input remote clients
            ClientSimulationManager.Instance.emulatedInputTanks[originalSenderId].ReceiveInputWindow(relayedInputWindow.ToArray());

            if (DEBUG_INPUT)
            {
                Debug.Log($"[{SimClock.TickCounter}]Recieved relayed input window from client {originalSenderId}: Ticks[{relayedInputWindow.First().timestamp}-{relayedInputWindow.Last().timestamp}]");
            }
        }

        public void BroadcastSimulationSnapshot(SimulationSnapshot snapshot)
        {
            SendSimulationSnapshot(snapshot, NetworkManager.Singleton.ConnectedClientsIds.Where(clId => clId != NetworkManager.ServerClientId).ToArray());
        }

        public void SendSimulationSnapshot(SimulationSnapshot snapshot, ulong[] targetClientIds)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Debug.LogException(new InvalidOperationException($"[{SimClock.TickCounter}]Client's shouldn't send simulation snapshots, only inputs."));
                return;
            }

            snapshot.status = SnapshotStatus.Authoritative;

            var writer = new FastBufferWriter(SimulationSnapshot.MAX_SERIALIZED_SIZE, Allocator.Temp);
            var customMessagingManager = NetworkManager.Singleton.CustomMessagingManager;

            using (writer)
            {
                writer.WriteValue(snapshot);
                customMessagingManager.SendNamedMessage(MessageName.SimulationSnapshot, targetClientIds, writer, NetworkDelivery.Unreliable);
            }

            for(int i = 0; i < targetClientIds.Length; i++)
            {
                SendThrottleSignal(targetClientIds[i]);
            }
            

            if (DEBUG_SNAPSHOTS) Debug.Log($"[{SimClock.TickCounter}]Sent snapshot[{snapshot.timestamp}] to ALL clients.");
        }

        public void ReceiveSimulationSnapshot(ulong serverId, FastBufferReader snapshotPayload)
        {
            // La loggeamos en lugar de lanzar la excepcion porque no queremos que malos actores nos manden paquetes por
            // el canal de SimulationSnapshot y nos tiren el server (lo mismo aplica a el cliente)!!!
            if (NetworkManager.Singleton.IsServer)
            {
                Debug.LogException(new InvalidOperationException($"[{SimClock.TickCounter}]The SERVER shouldn't receive simulation snapshots, only inputs."));
                return;
            }

            if (serverId != NetworkManager.ServerClientId)
            {
                Debug.LogException(new InvalidOperationException("Can only receive Simulation Snapshot messages from the server!"));
                return;
            }
            
            SimulationSnapshot snapshot = new SimulationSnapshot();
            snapshotPayload.ReadValue(out snapshot);

            if (DEBUG_SNAPSHOTS)
            {
                Debug.Log($"[{SimClock.TickCounter}]Received Snapshot:{snapshot}");
            }

            if (snapshot.status == SnapshotStatus.Authoritative)
            {
                ClientSimulationManager.Instance.EvaluateForReconciliation(snapshot);
            }

            // TESTING !!!!
            //ClientSimulationManager.Instance.SetSimulation(snapshot);

            if (DEBUG_SNAPSHOTS) Debug.Log($"[{SimClock.TickCounter}]Received snapshot[{snapshot.timestamp}] from server.");
        }
    }
}