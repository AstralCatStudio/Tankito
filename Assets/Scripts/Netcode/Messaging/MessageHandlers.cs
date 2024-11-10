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
    public class MessageName
    {
        public static string ClockSignal => "ClockSignal";
        public static string InputWindow => "InputWindow";
        public static string SimulationSnapshot => "SimulationSnapshot";
    }

    public class MessageHandlers : NetworkBehaviour
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

        /// <summary>
        /// For most cases, you want to register once your NetworkBehaviour's
        /// NetworkObject (typically in-scene placed) is spawned.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            // Both the server-host and client(s) register the custom named message.
            NetworkManager.CustomMessagingManager.RegisterNamedMessageHandler(MessageName.ClockSignal, RecieveClockSignal);
            NetworkManager.CustomMessagingManager.RegisterNamedMessageHandler(MessageName.InputWindow, ReceiveInputWindow);
            NetworkManager.CustomMessagingManager.RegisterNamedMessageHandler(MessageName.SimulationSnapshot, ReceiveSimulationSnapshot);
        }

        public override void OnNetworkDespawn()
        {
            // De-register when the associated NetworkObject is despawned.
            NetworkManager.CustomMessagingManager.UnregisterNamedMessageHandler(MessageName.ClockSignal);
            NetworkManager.CustomMessagingManager.UnregisterNamedMessageHandler(MessageName.InputWindow);
            NetworkManager.CustomMessagingManager.UnregisterNamedMessageHandler(MessageName.SimulationSnapshot);
        }

        public void SendClockSignal(ClockSignal signal)
        {
            if (!IsServer)
            {
                Debug.Log("Client's can't send clock signals");
            }
            
            
            var customMessagingManager = NetworkManager.CustomMessagingManager;
            var writer = new FastBufferWriter(FastBufferWriter.GetWriteSize(signal), Allocator.Temp);

            using (writer)
            {
                writer.WriteValue(signal);
                customMessagingManager.SendNamedMessageToAll(MessageName.ClockSignal, writer, NetworkDelivery.ReliableSequenced);
            }

            if (IsServer && !IsClient) ClockManager.Instance.StartClock();
            if (DEBUG_CLOCK) Debug.Log($"Sent clock signal: {signal}");
        }

        private void RecieveClockSignal(ulong serverId, FastBufferReader payload)
        {
            if (IsServer && !IsClient) return;
            
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
                    ClockManager.Instance.StartClock();
                    break;

                case ClockSignalHeader.Stop:
                    ClockManager.Instance.StopClock();
                    break;

                case ClockSignalHeader.Throttle:
                    ClockManager.Instance.ThrottleClock(signal.throttleTicks);
                    break;

                default:
                    throw new InvalidOperationException("Invalid clock signal header: " + signal.header);
            }

            if (DEBUG_CLOCK) Debug.Log($"Received clock signal: {signal}");
        }

        /// <summary>
        /// Invoke to send an <see cref="InputWindowBuffer.inputBuffer"/> to the server
        /// </summary>
        public void SendInputWindowToServer(CircularBuffer<InputPayload> inputWindow)
        {
            var writer = new FastBufferWriter(FastBufferWriter.GetWriteSize<InputPayload>()*inputWindow.Count, Allocator.Temp);
            var customMessagingManager = NetworkManager.CustomMessagingManager;

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
                string inputWindowTicks = "";
                foreach(var ip in inputWindow)
                {
                    inputWindowTicks += ip.timestamp + ((ip.timestamp != inputWindow.Last().timestamp) ? ", " : "");
                }
                if (DEBUG_INPUT) Debug.Log($"Sent input window: ticks({inputWindowTicks})");
            }
        }

        /// <summary>
        /// Invoked when receiving a custom message of type <see cref="MessageName.InputWindow"/>
        /// </summary>
        private void ReceiveInputWindow(ulong senderId, FastBufferReader payload)
        {
            //if (senderId != NetworkManager.LocalClientId && !ServerSimulationManager.Instance.remoteInputTanks.ContainsKey(senderId))
            //{
            //    Debug.LogWarning($"Client[{senderId}] is not registered in {ServerSimulationManager.Instance.remoteInputTanks}");
            //    return;
            //}

            List<InputPayload> receivedInputWindow = new List<InputPayload>();

            while (payload.TryBeginRead(FastBufferWriter.GetWriteSize<InputPayload>()))
            {
                InputPayload inputPayload;
                payload.ReadValue(out inputPayload);
                receivedInputWindow.Add(inputPayload);
            }

            if (IsServer)
            {
                // Relay Client inputs
                var relayWriter = new FastBufferWriter(payload.Length, Allocator.Temp);
                byte[] payloadBytes = new byte[payload.Length];
                payload.ReadBytes(ref payloadBytes, 0, payload.Length);

                using (relayWriter)
                {
                    relayWriter.WriteBytesSafe(payloadBytes);
                    var relayDestinations = NetworkManager.Singleton.ConnectedClientsIds.Where(id => id != senderId).ToArray();
                    NetworkManager.CustomMessagingManager.SendNamedMessage(MessageName.InputWindow, relayDestinations, relayWriter, NetworkDelivery.Unreliable);
                }

                // Store inputWindow
                if (senderId != NetworkManager.LocalClientId)
                {
                    ServerSimulationManager.Instance.remoteInputTanks[senderId].AddInput(receivedInputWindow.ToArray());

                    // Respond with throttling signal
                    int throttleTicks = ServerSimulationManager.Instance.remoteInputTanks[senderId].BufferSize-ServerSimulationManager.Instance.remoteInputTanks[senderId].IdealBufferSize;
                    var throttleSignal = new ClockSignal(ClockSignalHeader.Throttle, throttleTicks);
                    var throttleWriter = new FastBufferWriter(FastBufferWriter.GetWriteSize(throttleSignal), Allocator.Temp);

                    using (throttleWriter)
                    {
                        throttleWriter.WriteValue(throttleSignal);
                        NetworkManager.CustomMessagingManager.SendNamedMessage(MessageName.ClockSignal, senderId, throttleWriter, NetworkDelivery.Unreliable);
                    }
                }
            }
            else
            {
                // Store inputWindow
                ClientSimulationManager.Instance.emulatedInputTanks[senderId].ReceiveInputWindow(receivedInputWindow.ToArray());
            }

            if (DEBUG_INPUT)
            {
                string inputWindowTicks = "";
                foreach(var ip in receivedInputWindow)
                {
                    inputWindowTicks += ip.timestamp + ((ip.timestamp != receivedInputWindow.Last().timestamp) ? ", " : "");
                }
                Debug.Log($"Recieved input window from client {senderId}: ticks({inputWindowTicks})");
            }
        }

        public void SendSimulationSnapshot(GlobalSimulationSnapshot snapshot)
        {
            var writer = new FastBufferWriter(GlobalSimulationSnapshot.MAX_SERIALIZED_SIZE, Allocator.Temp);
            var customMessagingManager = NetworkManager.CustomMessagingManager;

            using (writer)
            {
                writer.WriteValue(snapshot);
                customMessagingManager.SendNamedMessageToAll(MessageName.InputWindow, writer, NetworkDelivery.Unreliable);
            }

            if (DEBUG_SNAPSHOTS) Debug.Log($"Sent snapshot[{snapshot.timestamp}] to ALL clients.");
        }

        private void ReceiveSimulationSnapshot(ulong serverId, FastBufferReader snapshotPayload)
        {
            if (serverId != NetworkManager.ServerClientId)
            {
                Debug.LogWarning("Can only receive Simulation Snapshot messages from the server!");
                return;
            }
            
            GlobalSimulationSnapshot snapshot;
            snapshotPayload.ReadValue(out snapshot);

            if (DEBUG_SNAPSHOTS) Debug.Log($"Received snapshot[{snapshot.timestamp}] from server.");
        }
    }
}