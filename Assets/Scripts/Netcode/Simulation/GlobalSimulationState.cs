using System.Collections;
using System.Collections.Generic;
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

    public struct GlobalSimulationSnapshot
    {
        public int timestamp;
        public SnapshotState state;
        public Dictionary<ASimulationObject, ISimulationState> objectSnapshots; // Se hace de  ISimulationState para poder mantenerlo generico entre cosas distintas, como balas que tan solo tienen un par de variables y los tanques, que tienen mas info
        
        public ISimulationState this[ASimulationObject obj]
        {
            get => objectSnapshots[obj];
            set => objectSnapshots[obj] = value;
        }

        public void Initialize()
        {
            objectSnapshots = new Dictionary<ASimulationObject, ISimulationState>();
        }
    }
}
