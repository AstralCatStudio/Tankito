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
    public class MessageName
    {
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
            NetworkManager.CustomMessagingManager.RegisterNamedMessageHandler(MessageName.InputWindow, ReceiveInputWindow);
            NetworkManager.CustomMessagingManager.RegisterNamedMessageHandler(MessageName.SimulationSnapshot, ReceiveSimulationSnapshot);
        }

        public override void OnNetworkDespawn()
        {
            // De-register when the associated NetworkObject is despawned.
            NetworkManager.CustomMessagingManager.UnregisterNamedMessageHandler(MessageName.InputWindow);
            NetworkManager.CustomMessagingManager.UnregisterNamedMessageHandler(MessageName.SimulationSnapshot);
        }

        /// <summary>
        /// Invoke to send an <see cref="InputWindowBuffer.inputBuffer"/> to the server
        /// </summary>
        public void SendInputWindowToServer(CircularBuffer<InputPayload> inputWindow, ulong networkObjectId)
        {
            var payload = inputWindow.ToArray();
            var writer = new FastBufferWriter(FastBufferWriter.GetWriteSize(payload), Allocator.Temp);
            var customMessagingManager = NetworkManager.CustomMessagingManager;
            using (writer)
            {
                writer.WriteValue(payload);
                customMessagingManager.SendNamedMessage(MessageName.InputWindow, NetworkManager.ServerClientId, writer, NetworkDelivery.Unreliable);
            }
        }

        /// <summary>
        /// Invoked when receiving a custom message of type <see cref="MessageName.InputWindow"/>
        /// </summary>
        private void ReceiveInputWindow(ulong senderId, FastBufferReader messagePayload)
        {
            if (!ServerSimulationManager.Instance.remoteInputTanks.ContainsKey(senderId))
                return;
                
            var receivedInputWindow = new InputPayload[InputWindowBuffer.WINDOW_SIZE];
            messagePayload.ReadValue(out receivedInputWindow);
            ServerSimulationManager.Instance.remoteInputTanks[senderId].AddInput(receivedInputWindow);
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