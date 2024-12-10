
using UnityEngine.UIElements;
using System;
using UnityEngine;

namespace Tankito.Netcode.Simulation
{
    public interface ISimulationState
    {
        static int SerializedSize { get; }
    }

}