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
        public ulong netObjectId;
        public SimulationObjectType simObjType;
        public ISimulationState state;

        public SimulationObjectUpdate(ASimulationObject simObj, ISimulationState newState)
        {
            netObjectId = simObj.NetworkObjectId;
            simObjType = GetType(simObj);
            this.state = newState;
        }

        public SimulationObjectUpdate(ulong netObjectId, ISimulationState state)
        {
            this.netObjectId = netObjectId;
            simObjType = SimulationObjectType.NULL;
            this.state = state;
        }

        public SimulationObjectUpdate(ulong netObjectId, SimulationObjectType simObjType, ISimulationState state)
        {
            this.netObjectId = netObjectId;
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
            serializer.SerializeValue(ref netObjectId);
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