using System.Collections;
using System.Collections.Generic;
using Tankito.Netcode.Simulation;
using UnityEngine;

public struct TankSimulationState : ISimulationState
{
    public Vector2 Position { private set; get; }
    public float Rotation { private set; get; }
    public Vector2 Velocity { private set; get; }
    public float TorretRotation { private set; get; }

    public TankSimulationState(Vector2 position, float rotation, Vector2 velocity, float torretRotation)
    {
        this.Position = position;
        this.Rotation = rotation;
        this.Velocity = velocity;
        this.TorretRotation = torretRotation;
    }
}