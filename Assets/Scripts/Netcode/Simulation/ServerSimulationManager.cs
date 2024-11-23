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
        /// Relates NetworkClientId(ulong) to a specific <see cref="RemoteTankInput"/>.  
        /// </summary>
        public Dictionary<ulong, RemoteTankInput> remoteInputTanks = new Dictionary<ulong,RemoteTankInput>();
        protected HashSet<ulong> m_removeFromSimQueue;

        void Start()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Debug.Log("Proceeding to destroy ServerSimulationManager (because it's NOT a SERVER)");
                Destroy(this);
            }

            m_removeFromSimQueue = new HashSet<ulong>();
        }
        
        public void QueueForRemoval(ulong simObjId)
        {
            if (m_simulationObjects.ContainsKey(simObjId))
            {
                m_removeFromSimQueue.Add(simObjId);
            }
            else
            {
                throw new IndexOutOfRangeException($"[{SimClock.TickCounter}]SimObjId({simObjId}) is not registered in simulation object dictionary!");
            }
        }

        public override void Simulate()
        {
            //Debug.Log("SERVER SimulationManager Simulate() called!");
            //Debug.Log("TODO: Implement Input Gathering and Client Throttling RPC's on Server.");
            //GatherPlayerInput(); // Samplear la ventana de inputs. Aqui tambien iria la logica de client throttling
            base.Simulate();

            foreach(var objId in m_removeFromSimQueue)
            {
                var obj = m_simulationObjects[objId];
                if (obj is BulletSimulationObject bullet)
                {
                    Debug.Log($"[{SimClock.TickCounter}]Called Despawn for {objId}");
                    bullet.OnNetworkDespawn();
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            m_removeFromSimQueue.Clear();
            
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