using Unity.Netcode;

namespace Tankito.Netcode.Messaging
{
    
    public enum ClockSignalHeader
    {
        Start,
        ACK_Start,
        Stop,
        ACK_Stop,

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

        public ClockSignal(ClockSignalHeader header, int throttleTicks)
        {
            this.header = header;
            this.throttleTicks = throttleTicks;
        }

        public override string ToString()
        {
            string res = "";
            res += header.ToString();
            if (header == ClockSignalHeader.Throttle) res += "-" + throttleTicks;
            return res;
        }
    }
}