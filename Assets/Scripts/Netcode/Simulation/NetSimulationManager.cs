using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tankito.Netcode.Simulation
{
    // Vale lo siento es un poco cancer lo de hacer la herencia de singleton asi pero esq me da palo reescribir por enesima vez la implementacion de Singleton
    // en las clases que heredan de NetSimulationManager....
    public abstract class NetSimulationManager<T> : Singleton<T> where T : Component
    {
        /// <summary>
        /// Key -> Associated SimulationObjectId
        /// Value -> ASimulationObject component
        /// </summary>
        protected Dictionary<ulong, ASimulationObject> m_simulationObjects;
        protected HashSet<ASimulationObject> m_addToSimQueue;
        protected HashSet<ulong> m_removeFromSimQueue;

        protected abstract int CaptureSnapshotTick { get; }

        public virtual ASimulationObject GetSimObj(ulong simObjId)
        {
            return m_simulationObjects[simObjId];
        }

        public bool ContainsSimObj(ulong simObjId)
        {
            return m_simulationObjects.ContainsKey(simObjId);
        }

        protected override void Awake()
        {
            base.Awake();
            m_simulationObjects = new Dictionary<ulong, ASimulationObject>();
            m_addToSimQueue = new HashSet<ASimulationObject>();
            m_removeFromSimQueue = new HashSet<ulong>();
        }

        public virtual void AddToSim(ASimulationObject obj)
        {
            if (obj.SimObjId == default) Debug.LogWarning($"[{SimClock.TickCounter}]SimObjId has not been initialized!!!");
            m_simulationObjects.Add(obj.SimObjId, obj);
        }
 
        public virtual void RemoveFromSim(ASimulationObject obj)
        {
            m_simulationObjects.Remove(obj.SimObjId);
        }

        public bool ContainsKey(ulong hash)
        {
            return m_simulationObjects.ContainsKey(hash);
        }

        private void OnEnable()
        {
            SimClock.OnTick += Simulate;
        }

        private void OnDisable()
        {
            SimClock.OnTick -= Simulate;
        }

        public void QueueForSpawn(ASimulationObject simObj)
        {
            if (!m_simulationObjects.ContainsKey(simObj.SimObjId))
            {
                m_addToSimQueue.Add(simObj);
            }
            else
            {
                throw new IndexOutOfRangeException($"[{SimClock.TickCounter}]SimObjId({simObj}) is already registered in simulation object dictionary!");
            }
        }

        public void QueueForDespawn(ulong simObjId)
        {
            //Debug.Log($"Queued [{simObjId}] for despawn");
            if (m_simulationObjects.ContainsKey(simObjId))
            {
                m_removeFromSimQueue.Add(simObjId);
            }
            else
            {
                throw new IndexOutOfRangeException($"[{SimClock.TickCounter}]SimObjId({simObjId}) is not registered in simulation object dictionary!");
            }
        }


        /// <summary>
        /// Advance the simulation forward by 1 simulation tick (simulation tick time is determined by <see cref="SimClock.SimDeltaTime" />).
        /// </summary>
        public virtual void Simulate()
        {
            //Debug.Log($"[{CaptureSnapshotTick}] Simulating");
            foreach (var obj in m_simulationObjects.Values)
            {
                //Debug.Log("computing kinematics for " + obj);
                obj.ComputeKinematics(SimClock.SimDeltaTime);
            }

            // This is placed here in order to act on SimObjs created during the ComputeKinematics phase (like bullets)
            foreach(var newObj in m_addToSimQueue)
            {
                //Debug.Log("adding " + newObj + " to sim and computing its kinematics");
                newObj.OnNetworkSpawn();
                newObj.ComputeKinematics(SimClock.SimDeltaTime);
            }
            m_addToSimQueue.Clear();
            
            //Debug.Log($"[{CaptureSnapshotTick}] Calling Physics2D.Simulate on simulation scene");
            Physics2D.Simulate(SimClock.SimDeltaTime);

            foreach (var objId in m_removeFromSimQueue)
            {
                var obj = m_simulationObjects[objId];
                if (obj is BulletSimulationObject bullet)
                {
                    //Debug.Log("removing " + objId + " from simulation scene");
                    bullet.OnNetworkDespawn();
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            m_removeFromSimQueue.Clear();
        }

        public SimulationSnapshot  CaptureSnapshot()
        {
            var newSnapshot = new SimulationSnapshot();
            newSnapshot.Initialize();
            newSnapshot.SetTimestamp(CaptureSnapshotTick);

            //Debug.Log(m_simulationObjects.Values.Count);
            foreach(var simObj in m_simulationObjects.Values)
            {
                newSnapshot[simObj] = (simObj.SimObjType, simObj.GetSimState());
            }

            //Debug.Log($"\t[{CaptureSnapshotTick}] Captured Snapshot: {newSnapshot}");
            
            return newSnapshot;
        }
    }
}