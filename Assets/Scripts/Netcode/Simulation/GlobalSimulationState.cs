using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Tankito.Netcode.Simulation
{
    public enum SnapshotState
    {
        /// <summary>
        /// When it's a client prediction of what the Snapshot at any given tick
        /// is going to look like in the server (guess at authoritative snapshot).
        /// </summary>
        Predicted,
        /// <summary>
        /// When the snapshot has been certified against the server's simulation.
        /// This happens when we receive server snapshots and we confirm our predictions were correct,
        /// or when we replace a miss prediction with an Authoritative snapshot. 
        /// </summary>
        Authoritative,
        /// <summary>
        /// When the snapshot has been checked as authoritative, but the server has also been notified
        /// that we acknowledge the reception of said authoritative snapshot. 
        /// </summary>
        Acknowledged
    }

    public struct SimulationSnapshot : INetworkSerializable
    {
        public int timestamp;
        public SnapshotState status; 
        private Dictionary<ASimulationObject, ISimulationState> objectStates; // Se hace de  ISimulationState para poder mantenerlo generico entre cosas distintas, como balas que tan solo tienen un par de variables y los tanques, que tienen mas info
        private int objCount;

        const int MAX_TANKS_IN_LOBBY = 6;
        const int MAX_PROJECTILES_IN_LOBBY = 60;
        public const int MAX_SERIALIZED_SIZE = TankSimulationState.MAX_SERIALIZED_SIZE*MAX_TANKS_IN_LOBBY + BulletSimulationState.MAX_SERIALIZED_SIZE*MAX_PROJECTILES_IN_LOBBY;

        public IEnumerable<ASimulationObject> Keys { get => objectStates.Keys; }
        public IEnumerable<ISimulationState> Values { get => objectStates.Values; }
        public int ObjCount { get => objCount; }

        public void Initialize()
        {
            objectStates = new Dictionary<ASimulationObject, ISimulationState>();
        }

        public ISimulationState this[ASimulationObject obj]
        {
            get => objectStates[obj];
            set { objectStates[obj] = value; objCount++; }
        }
        internal bool ContainsKey(ASimulationObject obj) { return objectStates.ContainsKey(obj); }
        internal bool ContainsValue(ISimulationState state) { return objectStates.ContainsValue(state); }


        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref timestamp);
            serializer.SerializeValue(ref objCount);

            foreach(var objStatePair in objectStates)
            {
                var newUpdate = new SimulationObjectUpdate(objStatePair.Key.NetworkObjectId, objStatePair.Value);
                newUpdate.SetType(objStatePair.Key);
                serializer.SerializeValue(ref newUpdate);
            }
        }
    }
}
