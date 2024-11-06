using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEditor.PackageManager.UI;
using UnityEngine;

namespace Tankito.Netcode.Simulation
{
    public class ClientSimulationManager : NetSimulationManager<ClientSimulationManager>
    {
        GlobalSimulationSnapshot m_authSnapshot;
        void Start()
        {
            if (!NetworkManager.Singleton.IsClient)
            {
                Debug.LogWarning("ClientSimulationManager is network node that is NOT a CLIENT (is server). this should not happen!");
                Destroy(this);
            }
            m_authSnapshot.Initialize();
        }

        public override void Simulate()
        {
            Debug.Log("CLIENT SimulationManager Simulate() called!");
            
            Debug.Log("TODO: Implement Input Sampling and Sending on client.");
            //SampleInput(); // Ensamblar ventana de inputs y mandarla al servidor.
            // Tambien dead reckoning!
            if (!NetworkManager.Singleton.IsServer)
            {
                // We don't want HOSTs to execute Simulate twice per tick.
                // They should only Simulate with server logic, no need for prediciton and/or simulation throttling.
                base.Simulate();
            }
        }

        public void Rollback()
        {
            foreach(var obj in simulationObjects)
            {
                obj.SetSimState(m_authSnapshot[obj]);
            }
            
            int rollbackCounter = m_authSnapshot.timestamp;
            while(rollbackCounter < ClockManager.TickCounter)
            {
                // TODO: Input Replay
                Physics2D.Simulate(ClockManager.SimDeltaTime);
                rollbackCounter++;
            }
        }

        
        [ContextMenu("TestGetSet")]
        public void TestGetSet()
        {
            simulationObjects[1].SetSimState(simulationObjects[0].GetSimState());
        }
    }
}