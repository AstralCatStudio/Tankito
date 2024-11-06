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
            foreach (var obj in simulationObjects.Where(obj => obj.IsKinematic))
            {
                obj.ComputeKinematics(ClockManager.SimDeltaTime);
            }

            Physics2D.Simulate(ClockManager.SimDeltaTime);
        }

        [ContextMenu("TestGetSet")]
        public void TestGetSet()
        {
            simulationObjects[1].SetSimState(simulationObjects[0].GetSimState());
        }

    }
}