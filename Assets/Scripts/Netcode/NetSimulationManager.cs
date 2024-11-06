using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tankito.Netcode.Simulation
{
    // Vale lo siento es un poco cancer lo de hacer la herencia de singleton asi pero esq me da palo reescribir por enesima vez la implementacion de Singleton
    // en las clases que heredan de NetSimulationManager....
    public abstract class NetSimulationManager<T> : Singleton<T> where T : Component
    {
        protected List<SimulationObject> simulationObjects;

        public virtual void AddSimulationObject(SimulationObject obj)
        {
            simulationObjects.Add(obj);
        }
 
        public virtual void RemoveSimulationObject(SimulationObject obj)
        {
            simulationObjects.Remove(obj);
        }

        private void OnEnable()
        {
            ClockManager.OnTick += Simulate;
        }

        private void OnDisable()
        {
            ClockManager.OnTick -= Simulate;
        }

        /// <summary>
        /// Advance the simulation forward by 1 simulation tick (simulation tick is determined by <see cref="ClockManager.SimDeltaTime" />).
        /// </summary>
        public virtual void Simulate()
        {
            foreach (SimulationObject obj in simulationObjects.Where(obj => obj.IsKinematic))
            {
                obj.ComputeKinematics();
            }

            Physics2D.Simulate(ClockManager.SimDeltaTime);
        } 

    }
}