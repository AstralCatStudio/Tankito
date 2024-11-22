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
        public ulong simObjId;
        public SimulationObjectType simObjType;
        public ISimulationState state;

        public SimulationObjectUpdate(ASimulationObject simObj, ISimulationState newState)
        {
            simObjId = simObj.SimObjId;
            simObjType = GetType(simObj);
            this.state = newState;
        }

        public SimulationObjectUpdate(ulong simObjId, ISimulationState state)
        {
            this.simObjId = simObjId;
            simObjType = SimulationObjectType.NULL;
            this.state = state;
        }

        public SimulationObjectUpdate(ulong simObjId, SimulationObjectType simObjType, ISimulationState state)
        {
            this.simObjId = simObjId;
            this.simObjType = simObjType;
            this.state = state;
        }

        private static SimulationObjectType GetType(ASimulationObject obj)
        {
            if (obj is TankSimulationObject)
            {
                return SimulationObjectType.Tank;
            }
            if (obj is BulletSimulationObject)
            {
                return SimulationObjectType.Bullet;
            }
            throw new ArgumentException($"Type of {obj} is not supported.");
        }
        public void SetType(in TankSimulationObject t) { simObjType = SimulationObjectType.Tank; }
        public void SetType(in BulletSimulationObject b)  { simObjType = SimulationObjectType.Bullet; }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref simObjId);
            serializer.SerializeValue(ref simObjType);
            switch(simObjType)
            {
                case SimulationObjectType.Tank:
                    TankSimulationState tankState = serializer.IsWriter ? (TankSimulationState)state : default;
                    tankState.NetworkSerialize(serializer);
                    if (serializer.IsReader) state = tankState;
                    break;

                case SimulationObjectType.Bullet:
                    BulletSimulationState bulletState = serializer.IsWriter ? ((BulletSimulationState)state) : default;
                    bulletState.NetworkSerialize(serializer);
                    if (serializer.IsReader) state = bulletState;
                    break;

                default:
                    throw new ArgumentNullException($"simObjType({simObjType} not implemented)");
            }
        }
    }
}