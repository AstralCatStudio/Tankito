using Unity.Netcode;

namespace Tankito.Netcode.Messaging
{
    
    public enum ClockSignalHeader
    {
        Start,
        Stop,
        Sync,
        Throttle,
        ACK_Start,
        ACK_Stop
    }

    public struct ClockSignal : INetworkSerializable
    {
        public ClockSignalHeader header;
        public int signalTicks;
        //public int serverTime; // BERNAT: Creo que probablemente no haga falta esto??

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref header);
            if (header == ClockSignalHeader.Throttle || header == ClockSignalHeader.Sync) // Conditional Serialization
            {
                serializer.SerializeValue(ref signalTicks);
                //serializer.SerializeValue(ref serverTime);
            }
        }

        public ClockSignal(ClockSignalHeader header, int signalTicks)//, int serverTime)
        {
            this.header = header;
            this.signalTicks = signalTicks;
            //this.serverTime = serverTime;
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