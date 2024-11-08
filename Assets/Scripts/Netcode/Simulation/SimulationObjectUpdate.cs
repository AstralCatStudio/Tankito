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

        public void SetType(ASimulationObject obj)
        {
            if (obj is TankSimulationObject)
            {
                simObjType = SimulationObjectType.Tank;
                return;
            }
            if (obj is BulletSimulationObject)
            {
                simObjType = SimulationObjectType.Bullet;
                return;
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
                    ((TankSimulationState)state).NetworkSerialize(serializer);
                    break;

                case SimulationObjectType.Bullet:
                    ((BulletSimulationState)state).NetworkSerialize(serializer);
                    break;

                default:
                    throw new ArgumentNullException($"simObjType({simObjType} not implemented)");
            }
        }
    }
}