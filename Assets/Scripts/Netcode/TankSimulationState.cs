using System.Collections;
using System.Collections.Generic;
using Tankito.Netcode.Simulation;
using UnityEngine;

public class TankSimulationState : ASimulationState
{
    public float TorretRotation {  get; private set; }

    public TankSimulationState(Vector2 position, float rotation, float velocity, float torretRotation) : base(position, rotation, velocity)
    {     
        this.TorretRotation = torretRotation;
    }
}
