using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tankito.Netcode.Simulation
{
    public struct GlobalSimulationSnapshot
    {
        public int timestamp;
        public Dictionary<ASimulationObject, ISimulationState> objectSnapshots; // Se hace de  ISimulationState para poder mantenerlo generico entre cosas distintas, como balas que tan solo tienen un par de variables y los tanques, que tienen mas info

        /*public GlobalSimulationState()
        {
            simulationObjects = new Dictionary<SimulationObject, ISimulationState>();
        }*/

        public void Initialize()
        {
            objectSnapshots = new Dictionary<ASimulationObject, ISimulationState> ();
        }
        
        public ISimulationState this[ASimulationObject obj]
        {
            get => objectSnapshots[obj];
            set => objectSnapshots[obj] = value;
        }
    }
}
