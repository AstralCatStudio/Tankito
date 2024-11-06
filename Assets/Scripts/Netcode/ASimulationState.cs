
using UnityEngine.UIElements;
using System;
using UnityEngine;

namespace Tankito.Netcode.Simulation
{
    public abstract class ASimulationState 
    {
        public Vector2 Position { private set; get; }
        public float Rotation { private set; get; }
        public float Velocity { private set; get;}

        public ASimulationState(Vector2 position, float rotation, float velocity)
        {
            this.Position = position;
            this.Rotation = rotation;
            this.Velocity = velocity;
        }
    }
}