using System.Collections.Generic;
using Unity.Netcode;

namespace Tankito.Netcode.Simulation
{
    public enum SnapshotStatus
    {
        /// <summary>
        /// When it's a client prediction of what the Snapshot at any given tick
        /// is going to look like in the server (guess at authoritative snapshot).
        /// </summary>
        Predicted = 1,

        /// <summary>
        /// When the snapshot has been certified against the server's simulation.
        /// This happens when we receive server snapshots and we confirm our predictions were correct,
        /// or when we replace a miss prediction with an Authoritative snapshot. 
        /// </summary>
        Authoritative = 2
    }

    public struct SimulationSnapshot : INetworkSerializable
    {
        public int timestamp;
        public SnapshotStatus status;
        private Dictionary<ulong, (SimulationObjectType type, ISimulationState state)> objectStates;

        //const int MAX_TANKS_IN_LOBBY = 4;
        //const int MAX_PROJECTILES_IN_LOBBY = 20;
        //public const int MAX_SERIALIZED_SIZE = TankSimulationState.MAX_SERIALIZED_SIZE*MAX_TANKS_IN_LOBBY + BulletSimulationState.MAX_SERIALIZED_SIZE*MAX_PROJECTILES_IN_LOBBY;

        public IEnumerable<ulong> IDs { get => objectStates.Keys; }
        public IEnumerable<(SimulationObjectType type, ISimulationState state)> States { get => objectStates.Values; }
        public int Count { get => objectStates.Count; }

        public void Initialize()
        {
            objectStates = new Dictionary<ulong, (SimulationObjectType type, ISimulationState state)>();
        }

        public (SimulationObjectType type, ISimulationState state) this[ASimulationObject obj]
        {
            get => objectStates[obj.SimObjId];
            set => objectStates[obj.SimObjId] = value;
        }

        public (SimulationObjectType type, ISimulationState state) this[ulong obj]
        {
            get => objectStates[obj];
            set => objectStates[obj] = value;
        }

        internal bool ContainsId(ulong obj) { return objectStates.ContainsKey(obj); }

        /// <summary>
        /// Returns the size of the struct after serializing it with <see cref="NetworkSerialize"/> 
        /// </summary>
        /// <returns></returns>
        public int GetSerializedSize()
        {
            // Header
            int bytes = sizeof(int) + // timestamp
                        sizeof(SnapshotStatus) + // status
                        sizeof(ushort); // number of objects
            
            // SimulationObjectUpdate
            foreach(var objState in objectStates.Values)
            {
                bytes += SimulationObjectUpdate.GetSerializedSize(in objState.state);
            }
                        
            return bytes;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            ushort nObjects = default;

            serializer.SerializeValue(ref timestamp);

            serializer.SerializeValue(ref status);

            if (serializer.IsWriter) nObjects = (ushort)objectStates.Keys.Count;

            serializer.SerializeValue(ref nObjects);
            
            if (serializer.IsWriter)
            {
                foreach(var objStatePair in objectStates)
                {
                    SimulationObjectUpdate simObjUpdate = new SimulationObjectUpdate(objStatePair.Key, objStatePair.Value, timestamp);
                    simObjUpdate.NetworkSerialize(serializer);
                }
            }
            else if (serializer.IsReader)
            {
                objectStates = new();
                for(int i=0; i < nObjects; i++)
                {
                    SimulationObjectUpdate simObjUpdate = new();
                    simObjUpdate.NetworkSerialize(serializer);

                    objectStates.Add(simObjUpdate.ID, (simObjUpdate.type, simObjUpdate.state));
                }
            }
        }

        public override string ToString()
        {
            var str = $"[Tick({timestamp})|Status({status})|Count({Count})]";

            foreach(var pair in objectStates)
            {
                str += $" ({pair.Key}) |";
            }
            str += ")";

            return str;
        }
    }
}
