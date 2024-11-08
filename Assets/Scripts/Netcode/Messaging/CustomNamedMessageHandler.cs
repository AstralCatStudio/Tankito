using System;
using UnityEngine;
using Unity.Collections;
using Unity.Netcode;
using Tankito.Utils;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using Tankito.Netcode.Simulation;

namespace Tankito.Netcode.Messaging
{
    public enum ClockSignalHeader
    {
        Start,
        Stop,
        Throttle
    }

    public struct ClockSignal : INetworkSerializable
    {
        public ClockSignalHeader header;
        public int throttleTicks;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref header);
            if (header == ClockSignalHeader.Throttle)
            {
                serializer.SerializeValue(ref throttleTicks);
            }
        }
    }

    public class MessageName
    {
        public static string ClockSignal => "ClockSignal";
        public static string InputWindow => "InputWindow";
        public static string SimulationSnapshot => "SimulationSnapshot";
    }

    public class CustomNamedMessageHandler : NetworkBehaviour
    {
        public static CustomNamedMessageHandler Instance;

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
                writer.WriteValue<ClockSignal>(signal);
                customMessagingManager.SendNamedMessageToAll(MessageName.ClockSignal, writer, NetworkDelivery.ReliableSequenced);
            }
        }

        private void RecieveClockSignal(ulong senderClientId, FastBufferReader payload)
        {
            if (senderClientId != NetworkManager.ServerClientId)
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
                    throw new InvalidOperationException($"{signal.header} is not a valid clock signal header!");
            }
        }

        /// <summary>
        /// Invoke to send an <see cref="InputWindowBuffer.inputBuffer"/> to the server
        /// </summary>
        public void SendInputWindowToServer(CircularBuffer<InputPayload> inputWindow)
        {
            var inputArr = inputWindow.ToArray();
            var writer = new FastBufferWriter(FastBufferWriter.GetWriteSize(inputArr), Allocator.Temp);
            var customMessagingManager = NetworkManager.CustomMessagingManager;

            using (writer)
            {
                writer.WriteValue(inputArr);
                customMessagingManager.SendNamedMessage(MessageName.InputWindow, NetworkManager.ServerClientId, writer, NetworkDelivery.Unreliable);
            }

            Debug.Log($"Sent input window to server : {inputWindow}");
        }

        /// <summary>
        /// Invoked when receiving a custom message of type <see cref="MessageName.InputWindow"/>
        /// </summary>
        private void ReceiveInputWindow(ulong senderId, FastBufferReader payload)
        {
            if (!ServerSimulationManager.Instance.remoteInputTanks.ContainsKey(senderId))
            {
                Debug.Log($"{senderId} is not registered in {ServerSimulationManager.Instance.remoteInputTanks}");
                return;
            }

            InputPayload[] receivedInputWindow = new InputPayload[InputWindowBuffer.WINDOW_SIZE];
            payload.ReadValue(out receivedInputWindow);

            ServerSimulationManager.Instance.remoteInputTanks[senderId].AddInput(receivedInputWindow);

            Debug.Log($"Recieved input window from client {senderId}: {receivedInputWindow}");
        }

        public void SendSimulationSnapshot(GlobalSimulationSnapshot snapshot)
        {
            throw new NotImplementedException();
        }

        private void ReceiveSimulationSnapshot(ulong senderClientId, FastBufferReader messagePayload)
        {
            throw new NotImplementedException();
        }

        public static byte[] ToBytes(System.Object obj)
        {
            if (obj == null)
                return null;
            
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);

            return ms.ToArray();
        }

        public static T ToObject<T>(byte[] arrBytes)
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            ms.Write(arrBytes, 0, arrBytes.Length);
            ms.Seek(0, SeekOrigin.Begin);
            T obj = (T) binForm.Deserialize(ms);

            return obj;
        }
    }
}