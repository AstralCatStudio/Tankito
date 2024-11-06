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
        

        public override void OnNetworkSpawn() // Should work and be called for pooled objects too!
        {
            if(IsServer)
            {
                ServerSimulationManager.Instance.AddSimulationObject(this);
            }
            //else  // Este else no lo queremos porque necesitamos que un host sea capaz de recoger el input del jugador,
                    // de lo cual se encarga ClientSimulationManager
            //{
                ClientSimulationManager.Instance.AddSimulationObject(this);
            //}
        }

        public override void OnNetworkDespawn() // Should work and be called for pooled objects too!
        {
            if(IsServer)
            {
                ServerSimulationManager.Instance.RemoveSimulationObject(this);
            }
            ClientSimulationManager.Instance.RemoveSimulationObject(this);
        }

        /// <summary>
        /// Calls funtions that push the physics variables of the simulation object to the physics engine kinemmatically,
        /// to then be set at the next physics tick.
        /// </summary>
        /// 
        internal void ComputeKinematics()
        {
            OnComputeKinematics?.Invoke();
        }

        // Bernat: Creo que estos metodos de iniciar reconciliacion y de reconciliar son tarea del simulation manager,
        //         lo unico que ofrece el SimulationObject es como una etiqueta que designa que el simulation manager tendra en cuenta al
        //         objeto a la hora de simular en red.
        // public abstract void InitReconcilation(ISimulationState simulationState);
        // public abstract void Reconciliate(int rewindTick);

    }
}