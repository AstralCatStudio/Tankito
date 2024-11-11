using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tankito.Netcode.Simulation
{
    // Vale lo siento es un poco cancer lo de hacer la herencia de singleton asi pero esq me da palo reescribir por enesima vez la implementacion de Singleton
    // en las clases que heredan de NetSimulationManager....
    public abstract class NetSimulationManager<T> : Singleton<T> where T : Component
    {
        protected List<ASimulationObject> simulationObjects;
        

        protected override void Awake()
        {
            base.Awake();
            simulationObjects = new List<ASimulationObject>();
        }

        public virtual void AddToSim(ASimulationObject obj)
        {
            simulationObjects.Add(obj);
        }
 
        public virtual void RemoveFromSim(ASimulationObject obj)
        {
            simulationObjects.Remove(obj);
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
            foreach (var obj in simulationObjects.Where(obj => obj.IsKinematic))
            {
                obj.ComputeKinematics(SimClock.SimDeltaTime);
                //Debug.Log($"ComputedKinematics for: {obj}");
            }

            Physics2D.Simulate(SimClock.SimDeltaTime);
        }

        public GlobalSimulationSnapshot CaptureSnapshot()
        {
            var newSnapshot = new GlobalSimulationSnapshot();
            newSnapshot.Initialize();
            newSnapshot.timestamp = SimClock.TickCounter;
            
            foreach(var simObj in simulationObjects)
            {
                newSnapshot[simObj] = simObj.GetSimState();
            }
            
            return newSnapshot;
        }
    }
}