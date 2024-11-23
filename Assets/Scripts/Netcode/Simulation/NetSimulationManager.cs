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

        public virtual ASimulationObject GetSimObj(ulong simObjId)
        {
            return m_simulationObjects[simObjId];
        }

        public bool HasSimObj(ulong simObjId)
        {
            return m_simulationObjects.ContainsKey(simObjId);
        }
        

        protected override void Awake()
        {
            base.Awake();
            m_simulationObjects = new Dictionary<ulong, ASimulationObject>();
        }

        public virtual void AddToSim(ASimulationObject obj)
        {
            if (obj.SimObjId == default) Debug.LogWarning($"[{SimClock.TickCounter}]SimObjId has not been initialized!!!");
            m_simulationObjects.TryAdd(obj.SimObjId, obj);
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

        /// <summary>
        /// Advance the simulation forward by 1 simulation tick (simulation tick is determined by <see cref="SimClock.SimDeltaTime" />).
        /// </summary>
        public virtual void Simulate()
        {
            List<ASimulationObject> simulationObjectsSnapshot = m_simulationObjects.Values.ToList<ASimulationObject>();
            foreach (var obj in simulationObjectsSnapshot)
            {
                obj?.ComputeKinematics(SimClock.SimDeltaTime);
                //Debug.Log($"ComputedKinematics for: {obj}");
            }

            Physics2D.Simulate(SimClock.SimDeltaTime);
        }

        public SimulationSnapshot CaptureSnapshot()
        {
            var newSnapshot = new SimulationSnapshot();
            newSnapshot.Initialize();
            newSnapshot.timestamp = SimClock.TickCounter;

            //Debug.Log(m_simulationObjects.Values.Count);
            foreach(var simObj in m_simulationObjects.Values)
            {
                newSnapshot[simObj] = simObj.GetSimState();
            }
            
            return newSnapshot;
        }
    }
}