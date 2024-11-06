using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEditor.PackageManager.UI;
using UnityEngine;

namespace Tankito.Netcode.Simulation
{
    public class ClientSimulationManager : NetSimulationManager<ClientSimulationManager>
    {
        GlobalSimulationState m_authState;
        void Start()
        {
            if (!NetworkManager.Singleton.IsClient)
            {
                Debug.LogWarning("ClientSimulationManager is network node that is NOT a CLIENT (is server). this should not happen!");
            }
            m_authState.Initialize();
        }

        public override void Simulate()
        {
            Debug.Log("CLIENT SimulationManager Simulate() called!");
            
            Debug.Log("TODO: Implement Input Sampling and Sending on client.");
            //SampleInput(); // Ensamblar ventana de inputs y mandarla al servidor.
            if (!NetworkManager.Singleton.IsServer)
            {
                // We don't want HOSTs to execute Simulate twice per tick.
                // They should only Simulate with server logic, no need for prediciton and/or simulation throttling.
                base.Simulate();
            }
        }

        public void GeneralRollback()
        {
            foreach(SimulationObject obj in simulationObjects.Where(obj => obj.IsKinematic))
            {
                obj.InitReconcilation(m_authState.simulationObjects[obj]);
            }
            int rewindCounter = m_authState.simulationTick;
            while(rewindCounter < ClockManager.TickCounter)
            {
                foreach (SimulationObject obj in simulationObjects.Where(obj => obj.IsKinematic))
                {
                    obj.Reconciliate(rewindCounter);
                }
                Physics2D.Simulate(ClockManager.SimDeltaTime); //Esto va haber que cambiarlo por si se ha producido cambios en la velocidad 
                                                               //de la simulacion
                rewindCounter++;
            }
        }
    }
}