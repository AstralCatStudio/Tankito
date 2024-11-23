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

        public SimulationObjectUpdate(ulong simObjId, (SimulationObjectType simObjType, ISimulationState iState) state)
        {
            this.ID = simObjId;
            this.type = state.simObjType;
            this.state = state.iState;
        }

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
                    if (serializer.IsReader) state = tankState;
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