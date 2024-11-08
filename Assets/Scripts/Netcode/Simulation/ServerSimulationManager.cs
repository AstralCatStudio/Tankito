using System.Collections.Generic;
using Tankito.Netcode.Messaging;
using Unity.Netcode;
using UnityEngine;

namespace Tankito.Netcode.Simulation
{
    public class ServerSimulationManager : NetSimulationManager<ServerSimulationManager>
    {
        /// <summary>
        /// Relates NetworkClientId(ulong) to a specific <see cref="RemoteTankInput"/>.  
        /// </summary>
        public Dictionary<ulong, RemoteTankInput> remoteInputTanks = new Dictionary<ulong,RemoteTankInput>();
        

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

        #region DEBUG_TESTING_METHODS

        [ContextMenu("Send ClockSignal.Start")]
        public void SendClockSignalStart()
        {
            Debug.Log("Sending ClockSignal.Start broadcast");
            ClockSignal signal = new ClockSignal();
            signal.header = ClockSignalHeader.Start;
            CustomNamedMessageHandler.Instance.SendClockSignal(signal);
        }

        [ContextMenu("Send ClockSignal.Stop")]
        public void SendClockSignalStop()
        {
            Debug.Log("Sending ClockSignal.Start broadcast");
            ClockSignal signal = new ClockSignal();
            signal.header = ClockSignalHeader.Stop;
            CustomNamedMessageHandler.Instance.SendClockSignal(signal);
        }

        #endregion
    }
}