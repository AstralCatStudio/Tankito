using System;
using Unity.Netcode;
using UnityEngine;

namespace Tankito.Netcode.Simulation
{
    public abstract class SimulationObject : NetworkBehaviour
    {
        public bool IsKinematic {get => m_isKinematic; set => m_isKinematic = value; }
        [SerializeField]
        private bool m_isKinematic;

        // Define a delegate for the kinematics computation
        public delegate void KinematicFunction();

        // Define an event based on the delegate
        public event KinematicFunction OnComputeKinematics;
        
        /// <summary>
        /// Calls funtions that push the physics variables of the simulation object to the physics engine kinemmatically,
        /// to then be set at the next physics tick.
        /// </summary>
        /// 

        public virtual void OnNetorkSpawn()
        {
            if(IsServer)
            {
                ServerSimulationManager.Instance.AddSimulationObject(this);
            }
            else
            {
                ClientSimulationManager.Instance.AddSimulationObject(this);
            }
        }

        internal void ComputeKinematics()
        {
            OnComputeKinematics?.Invoke();
        }

        public abstract void InitReconcilation(ISimulationState simulationState);
        public abstract void Reconciliate(int rewindTick);
    }
}