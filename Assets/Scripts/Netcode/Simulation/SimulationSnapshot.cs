using System;
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
        /// Prediction of aut state calculated with full knowledge of players' inputs.
        /// </summary>
        CompletePrediction = 2,

        /// <summary>
        /// When the snapshot has been certified against the server's simulation.
        /// This happens when we receive server snapshots and we confirm our predictions were correct,
        /// or when we replace a miss prediction with an Authoritative snapshot. 
        /// </summary>
        Authoritative = 3,
    }

    public struct SimulationSnapshot : INetworkSerializable
    {
        private int m_timestamp;
        private SnapshotStatus m_status;
        private Dictionary<ulong, (SimulationObjectType type, ISimulationState state)> m_objectStates;

        //const int MAX_TANKS_IN_LOBBY = 4;
        //const int MAX_PROJECTILES_IN_LOBBY = 20;
        //public const int MAX_SERIALIZED_SIZE = TankSimulationState.MAX_SERIALIZED_SIZE*MAX_TANKS_IN_LOBBY + BulletSimulationState.MAX_SERIALIZED_SIZE*MAX_PROJECTILES_IN_LOBBY;

        public IEnumerable<ulong> IDs { get => m_objectStates.Keys; }
        public IEnumerable<(SimulationObjectType type, ISimulationState state)> States { get => m_objectStates.Values; }
        public int Count { get => m_objectStates.Count; }
        public SnapshotStatus Status { get => m_status; }
        public int Timestamp { get => m_timestamp; }

        public void SetTimestamp(int newTimestamp) { m_timestamp = newTimestamp; }
        public void SetStatus(SnapshotStatus newStatus) { m_status = newStatus; }

        public void Initialize()
        {
            m_objectStates = new Dictionary<ulong, (SimulationObjectType type, ISimulationState state)>();
        }

        public (SimulationObjectType type, ISimulationState state) this[ASimulationObject obj]
        {
            get => m_objectStates[obj.SimObjId];
            set => m_objectStates[obj.SimObjId] = value;
        }

        public (SimulationObjectType type, ISimulationState state) this[ulong obj]
        {
            get
            {
                return m_objectStates[obj];
            }

            set
            {
                m_objectStates[obj] = value;
            }
        }

        internal bool ContainsId(ulong obj) { return m_objectStates.ContainsKey(obj); }

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
            foreach(var objState in m_objectStates.Values)
            {
                bytes += SimulationObjectUpdate.GetSerializedSize(in objState.state);
            }
                        
            return bytes;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            ushort nObjects = default;

            serializer.SerializeValue(ref m_timestamp);

            serializer.SerializeValue(ref m_status);

            if (serializer.IsWriter) nObjects = (ushort)m_objectStates.Keys.Count;

            serializer.SerializeValue(ref nObjects);
            
            if (serializer.IsWriter)
            {
                foreach(var objStatePair in m_objectStates)
                {
                    SimulationObjectUpdate simObjUpdate = new SimulationObjectUpdate(objStatePair.Key, objStatePair.Value, Timestamp);
                    simObjUpdate.NetworkSerialize(serializer);
                }
            }
            else if (serializer.IsReader)
            {
                m_objectStates = new();
                for(int i=0; i < nObjects; i++)
                {
                    SimulationObjectUpdate simObjUpdate = new();
                    simObjUpdate.NetworkSerialize(serializer);

                    m_objectStates.Add(simObjUpdate.ID, (simObjUpdate.type, simObjUpdate.state));
                }
            }
        }

        public override string ToString()
        {
            var str = $"[Tick({Timestamp})|Status({Status})|Count({Count})]\n";

            foreach(var pair in m_objectStates)
            {
                str += $" ({pair.Key}): {pair.Value.state}\n";
            }
            str += $"---- end of snapshot({Timestamp}) ----";

            return str;
        }

        internal SimulationSnapshot InterpolateTo(SimulationSnapshot nextSnapshot, float t)
        {
            SimulationSnapshot interpolatedSnapshot = new SimulationSnapshot();
            interpolatedSnapshot.Initialize();
            interpolatedSnapshot.SetStatus(nextSnapshot.Status);
            interpolatedSnapshot.SetTimestamp(Timestamp);

            foreach(var pair in nextSnapshot.m_objectStates)
            {
                if (m_objectStates.ContainsKey(pair.Key))
                {
                    interpolatedSnapshot[pair.Key] = m_objectStates[pair.Key].InterpolateTo(pair.Value, t);
                }
                else
                {
                    interpolatedSnapshot[pair.Key] = pair.Value;
                }
            }

            return interpolatedSnapshot;
        }
    }
}
