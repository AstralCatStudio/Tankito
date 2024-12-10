using Unity.Netcode;

namespace Tankito.Netcode.Messaging
{
    
    public enum ClockSignalHeader
    {
        Start,
        Stop,
        Sync,
        SyncRequest,
        Throttle,
    }

    public struct ClockSignal : INetworkSerializable
    {
        public ClockSignalHeader header;
        public int signalTicks;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref header);
            if (header == ClockSignalHeader.Throttle || header == ClockSignalHeader.Sync) // Conditional Serialization
            {
                serializer.SerializeValue(ref signalTicks);
            }
        }

        public ClockSignal(ClockSignalHeader header, int signalTicks)
        {
            this.header = header;
            this.signalTicks = signalTicks;
        }

        public override string ToString()
        {
            string res = "";
            res += header.ToString();
            if (header == ClockSignalHeader.Throttle ||
                header == ClockSignalHeader.Sync)
                 res += ": " + signalTicks;
            return res;
        }
    }
}