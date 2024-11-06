using System.Collections;
using System.Collections.Generic;
using Tankito.Netcode.Simulation;
using UnityEngine;

public struct BulletSimulationState : ISimulationState
{
    public Vector2 Position { private set; get; }
    public float Rotation { private set; get; }
    public Vector2 Velocity { private set; get; }
    public float LifeTime { private set; get; } // In seconds

    public BulletSimulationState(Vector2 position, float rotation, Vector2 velocity, float lifeTime)
    {
        this.Position = position;
        this.Rotation = rotation;
        this.Velocity = velocity;
        this.LifeTime = lifeTime;
    }
}
