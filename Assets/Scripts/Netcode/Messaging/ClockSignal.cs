using Unity.Netcode;

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
            if (header == ClockSignalHeader.Throttle) // Conditional Serialization
            {
                serializer.SerializeValue(ref throttleTicks);
            }
        }
    }
}