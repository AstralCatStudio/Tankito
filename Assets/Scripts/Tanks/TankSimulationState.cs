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
        public TankAction PerformedAction { get => performedAction; }
        public PlayerState PlayerState { get => playerState; }
        public int StateInitTick { get => stateInitTick; }

        private Vector2 position;
        private float hullRotation;
        private Vector2 velocity;
        private float turretRotation;
        private TankAction performedAction;
        private PlayerState playerState;
        private int stateInitTick;

        //private TankTolerance tolerances;

        public const int MAX_SERIALIZED_SIZE = sizeof(float)*2 + sizeof(float)*2*2;

        public TankSimulationState(Vector2 position, float hullRotation, Vector2 velocity, float turretRotation, TankAction performedAction, PlayerState playerState, int stateInitTick)
        {
            this.position = position;
            this.hullRotation = hullRotation;
            this.velocity = velocity;
            this.turretRotation = turretRotation;
            this.performedAction = performedAction;
            this.playerState = playerState;
            this.stateInitTick = stateInitTick;
        }

        internal void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref position);
            serializer.SerializeValue(ref hullRotation);
            serializer.SerializeValue(ref velocity);
            serializer.SerializeValue(ref turretRotation);
            serializer.SerializeValue(ref performedAction);
            serializer.SerializeValue(ref playerState);
            serializer.SerializeValue(ref stateInitTick);
        }
    }
}