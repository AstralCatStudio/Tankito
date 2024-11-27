using System;
using Unity.Netcode;

namespace Tankito.Netcode.Simulation
{
    public enum SimulationObjectType
    {
        NULL,
        Tank,
        Bullet
    }

    public struct SimulationObjectUpdate : INetworkSerializable
    {
        public ulong ID;
        public SimulationObjectType type;
        public ISimulationState state;
        
        /// <summary>
        /// Used to calculate absolute ticks from relative values (fire, parry, dash).
        /// <para/>NOT MEANT TO BE SERIALIZED DIRECTLY.
        /// </summary>
        private int m_timestamp;

        public SimulationObjectUpdate(ulong simObjId, (SimulationObjectType simObjType, ISimulationState iState) state, int timestamp)
        {
            this.ID = simObjId;
            this.type = state.simObjType;
            this.state = state.iState;
            m_timestamp = timestamp;
        }

        public static int GetSerializedSize(in ISimulationState simState)
        {
            int headerSize = HEADER_SIZE;
            int stateSize;
            if (simState is TankSimulationState)
            {
                stateSize = TankSimulationState.SerializedSize;
            }
            else if (simState is BulletSimulationState)
            {
                stateSize = BulletSimulationState.SerializedSize;
            }
            else
            {
                throw new NotImplementedException($"Not implemented for {simState}");
            }

            return headerSize + stateSize;
        }
        const int HEADER_SIZE = sizeof(ulong) + sizeof(SimulationObjectType);


        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ID);
            serializer.SerializeValue(ref type);
            switch(type)
            {
                case SimulationObjectType.Tank:
                    // Asymmetric logic in serialization for Bidirectional func.
                    TankSimulationState tankState = serializer.IsWriter ? (TankSimulationState)state : default;
                    tankState.NetworkSerialize(serializer);
                    if (serializer.IsReader)
                    {
                        tankState.Timestamp = m_timestamp;
                        state = tankState;
                    }
                    break;

                case SimulationObjectType.Bullet:
                    // Asymmetric logic in serialization for Bidirectional func.
                    BulletSimulationState bulletState = serializer.IsWriter ? ((BulletSimulationState)state) : default;
                    bulletState.NetworkSerialize(serializer);
                    if (serializer.IsReader) state = bulletState;
                    break;

                default:
                    throw new ArgumentNullException($"simObjType({type} not implemented)");
            }
        }
    }
}