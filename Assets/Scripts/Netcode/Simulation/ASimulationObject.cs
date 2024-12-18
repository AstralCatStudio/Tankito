using System;
using Unity.Netcode;
using UnityEngine;

namespace Tankito.Netcode.Simulation
{
    public abstract class ASimulationObject : NetworkBehaviour
    {
        public ulong SimObjId => m_simObjId;
        public abstract SimulationObjectType SimObjType { get; }
        public abstract Vector2 Position { get; }

        [SerializeField] // <-- FOR DEBUG ONLY (don't modify, just observe)
        private ulong m_simObjId;
        
        // Define a delegate for the kinematics computation
        public delegate void KinematicFunction(float deltaTime);

        // Define an event based on the delegate
        public event KinematicFunction OnComputeKinematics;

        public void SetSimObjId(ulong simObjId) { m_simObjId = simObjId; }

        public void GenerateSimObjId(ulong ownerId, int tick, int genN)
        {
            m_simObjId = SimExtensions.HashSimObj(ownerId, tick, genN);
        }

        /// <summary>
        /// Automatically adds the simulation object from the local <see cref="Simulation.NetSimulationManager"/>.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            if(NetworkManager.Singleton.IsServer)
            {
                ServerSimulationManager.Instance.AddToSim(this);
            }
            else
            {
                ClientSimulationManager.Instance.AddToSim(this);
            }
        }

        /// <summary>
        /// Automatically removes the simulation object from the local <see cref="Simulation.NetSimulationManager"/>.
        /// </summary>
        public override void OnNetworkDespawn()
        {
            if(NetworkManager.Singleton.IsServer)
            {
                ServerSimulationManager.Instance.RemoveFromSim(this);
            }
            else
            {
                ClientSimulationManager.Instance.RemoveFromSim(this);
            }
        }

        /// <summary>
        /// Calls funtions that push the physics variables of the simulation object to the physics engine kinemmatically,
        /// to then be set at the next physics tick.
        /// </summary>
        /// 
        internal void ComputeKinematics(float deltaTime)
        {
            OnComputeKinematics?.Invoke(deltaTime);
        }

        public abstract ISimulationState GetSimState();

        /// <summary>
        /// It is recommended that you set the transform of objects instead of their rigidbodies,
        /// since rigidboy transformations are not changed until the next Physics update.
        /// </summary>
        /// <param name="state"></param>
        public abstract void SetSimState(in ISimulationState state);

        public override string ToString()
        {
            return "["+SimObjId.ToString()+"]";
        }
    }
}