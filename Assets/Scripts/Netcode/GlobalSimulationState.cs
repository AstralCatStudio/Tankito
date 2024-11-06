using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tankito.Netcode.Simulation
{
    public struct GlobalSimulationState
    {
        public int simulationTick;
        public Dictionary<ASimulationObject, ISimulationState> simulationObjects; // Se hace de  ISimulationState para poder mantenerlo generico entre cosas distintas, como balas que tan solo tienen un par de variables y los tanques, que tienen mas info

        /*public GlobalSimulationState()
        {
            simulationObjects = new Dictionary<SimulationObject, ISimulationState>();
        }*/

        public void Initialize()
        {
            simulationObjects = new Dictionary<ASimulationObject, ISimulationState> ();
        }
    }
}
