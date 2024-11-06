using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Tankito.Netcode.Simulation
{
    public class ServerSimulationManager : NetSimulationManager<ServerSimulationManager>
    {
        void Start()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Debug.LogWarning("Proceeding to destroy ServerSimulationManager (because it's NOT a SERVER)");
                Destroy(this);
            }
        }

        public override void Simulate()
        {
            Debug.Log("SERVER SimulationManager Simulate() called!");
            Debug.Log("TODO: Implement Input Gathering and Client Throttling RPC's on Server.");
            //GatherPlayerInput(); // Samplear la ventana de inputs. Aqui tambien iria la logica de client throttling
            base.Simulate();
        }
    }
}