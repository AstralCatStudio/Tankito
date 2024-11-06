using System.Collections;
using System.Collections.Generic;
using Tankito.Netcode.Simulation;
using UnityEngine;

public class BulletSimulationState : ASimulationState
{
    public int LifeTime {  get; private set; }

    public BulletSimulationState(Vector2 position, float rotation, float velocity, int lifeTime) : base(position, rotation, velocity)
    {
        this.LifeTime = lifeTime;
    }
}
