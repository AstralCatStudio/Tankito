
using UnityEngine.UIElements;
using System;
using UnityEngine;

namespace Tankito.Netcode.Simulation
{
    public interface ISimulationState 
    {
        // Reconciliation is not a responsibilty of Simulation States, they shouldn't even be aware of the concept of reconciliation
        // public bool CheckReconcilation(ISimulationState state);
    }

}