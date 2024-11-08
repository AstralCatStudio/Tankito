using System;
using System.Collections;
using System.Collections.Generic;
using Tankito.Netcode.Simulation;
using Unity.Netcode;
using UnityEngine;

namespace Tankito.Netcode.Simulation
{
    public struct TankSimulationState : ISimulationState
    {
        public Vector2 Position { get => position; }
        public float HullRotation { get => hullRotation; }
        public Vector2 Velocity { get => velocity; }
        public float TurretRotation { get => turretRotation; }

        private Vector2 position;
        private float hullRotation;
        private Vector2 velocity;
        private float turretRotation;

        public TankSimulationState(Vector2 position, float hullRotation, Vector2 velocity, float turretRotation)
        {
            this.position = position;
            this.hullRotation = hullRotation;
            this.velocity = velocity;
            this.turretRotation = turretRotation;
        }

        internal void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref position);
            serializer.SerializeValue(ref hullRotation);
            serializer.SerializeValue(ref velocity);
            serializer.SerializeValue(ref turretRotation);
        }
    }
}