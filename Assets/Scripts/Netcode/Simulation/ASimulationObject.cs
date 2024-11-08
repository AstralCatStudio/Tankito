using System;
using Unity.Netcode;
using UnityEngine;

namespace Tankito.Netcode.Simulation
{
    public abstract class ASimulationObject : NetworkBehaviour, INetworkSerializable
    {
        public bool IsKinematic {get => m_isKinematic; set => m_isKinematic = value; }
        [SerializeField]
        private bool m_isKinematic;

        // Define a delegate for the kinematics computation
        public delegate void KinematicFunction(float deltaTime);

        // Define an event based on the delegate
        public event KinematicFunction OnComputeKinematics;
        

        public override void OnNetworkSpawn() // Should work and be called for pooled objects too!
        {
            if(IsServer)
            {
                ServerSimulationManager.Instance.AddToSim(this);
            }
            //else  // Este else no lo queremos porque necesitamos que un host sea capaz de recoger el input del jugador,
                    // de lo cual se encarga ClientSimulationManager
            //{
                ClientSimulationManager.Instance.AddToSim(this);
            //}
        }

        public override void OnNetworkDespawn() // Should work and be called for pooled objects too!
        {
            if(IsServer)
            {
                ServerSimulationManager.Instance.RemoveFromSim(this);
            }
            ClientSimulationManager.Instance.RemoveFromSim(this);
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

        // Bernat: Creo que estos metodos de iniciar reconciliacion y de reconciliar son tarea del simulation manager,
        //         lo unico que ofrece el SimulationObject es como una etiqueta que designa que el simulation manager tendra en cuenta al
        //         objeto a la hora de simular en red.
        // public abstract void InitReconcilation(ISimulationState simulationState);
        // public abstract void Reconciliate(int rewindTick);

        public abstract ISimulationState GetSimState();
        public abstract void SetSimState(in ISimulationState state);

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            ulong objectId = NetworkObjectId;
            serializer.SerializeValue(ref objectId);
        }
    }
}