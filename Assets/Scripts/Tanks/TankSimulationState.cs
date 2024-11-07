using System.Collections;
using System.Collections.Generic;
using Tankito.Netcode.Simulation;
using UnityEngine;

namespace Tankito.Netcode.Simulation
{
        // POSSIBLE OPTIMIZATION: make struct immutable by marking struct readonly
    public /*readonly*/ struct TankSimulationState : ISimulationState
    {
        public Vector2 Position { get; private set; }
        public float HullRotation { get; private set; }
        public Vector2 Velocity { get; private set; }
        public float TurretRotation { get; private set; }

        public TankSimulationState(Vector2 position, float hullRotation, Vector2 velocity, float turretRotation)
        {
            this.Position = position;
            this.HullRotation = hullRotation;
            this.Velocity = velocity;
            this.TurretRotation = turretRotation;
        }
    }
}