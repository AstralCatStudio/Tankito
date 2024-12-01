using System;
using System.Collections.Generic;
using System.Linq;
using Tankito.Netcode.Messaging;
using Unity.Netcode;
using UnityEngine;

namespace Tankito.Netcode.Simulation
{
    public class ServerSimulationManager : NetSimulationManager<ServerSimulationManager>
    {
        /// <summary>
        /// Relates <see cref="NetworkBehaviour.OwnerClientId"/> to a specific <see cref="RemoteTankInput"/>.  
        /// </summary>
        public Dictionary<ulong, RemoteTankInput> remoteInputTanks = new Dictionary<ulong,RemoteTankInput>();

        protected override int CaptureSnapshotTick { get => SimClock.TickCounter; }

        void Start()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Debug.Log("Proceeding to destroy ServerSimulationManager (because it's NOT a SERVER)");
                Destroy(this);
            }
        }

        public override void Simulate()
        {
            //Debug.Log("SERVER SimulationManager Simulate() called!");
            //Debug.Log("TODO: Implement Input Gathering and Client Throttling RPC's on Server.");
            //GatherPlayerInput(); // Samplear la ventana de inputs. Aqui tambien iria la logica de client throttling
            base.Simulate();
            
            MessageHandlers.Instance.BroadcastSimulationSnapshot(CaptureSnapshot());
        }

        #region DEBUG_TESTING_METHODS

        [ContextMenu("Send ClockSignal.Start")]
        public void SendClockSignalStart()
        {
            //Debug.Log("Sending ClockSignal.Start broadcast");
            ClockSignal signal = new ClockSignal();
            signal.header = ClockSignalHeader.Start;
            MessageHandlers.Instance.SendClockSignal(signal);
        }

        [ContextMenu("Send ClockSignal.Stop")]
        public void SendClockSignalStop()
        {
            //Debug.Log("Sending ClockSignal.Start broadcast");
            ClockSignal signal = new ClockSignal();
            signal.header = ClockSignalHeader.Stop;
            MessageHandlers.Instance.SendClockSignal(signal);
        }

        [ContextMenu("Broadcast Last Snapshot")]
        public void BroadcastLastSnapshot()
        {
            MessageHandlers.Instance.BroadcastSimulationSnapshot(CaptureSnapshot());
        }
        
        [ContextMenu("PrintRegisteredIDs")]
        public void PrintRegisteredIDs()
        {
            var debug = $"[{SimClock.TickCounter}]Registered SimObjIds:[ ";
            foreach(var objId in m_simulationObjects.Keys)
            {
                debug += objId + (objId == m_simulationObjects.Keys.Last() ? " ]" : " , ");
            }
            Debug.Log(debug);
        }

        #endregion
    }
}