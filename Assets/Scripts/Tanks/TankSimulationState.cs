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

        /// <summary>
        /// Used to calculate absolute ticks (fire, parry, dash).
        /// <para/>NOT MEANT TO BE SERIALIZED DIRECTLY. Use
        /// <see cref="Timestamp"/> instead. 
        /// </summary>
        private int m_timestamp;
        public int Timestamp { private get => (m_timestamp >= 0) ? m_timestamp : throw new InvalidOperationException("m_timestamp is uninitialized");
                                        set => m_timestamp = value; }
        public int LastFireTick { get => Timestamp - ticksSinceFire; }
        public int LastDashTick { get => Timestamp - ticksSinceDash; }
        public int LastParryTick { get => Timestamp - ticksSinceParry; }

        public PlayerState PlayerState { get => playerState; }
        public int TicksSinceFire { get => ticksSinceFire; }
        public int TicksSinceDash { get => ticksSinceDash; }
        public int TicksSinceParry { get => ticksSinceParry; }

        private Vector2 position;
        private float hullRotation;
        private Vector2 velocity;
        private float turretRotation;
        private TankAction performedAction;
        private PlayerState playerState;
        private ushort ticksSinceFire;
        private ushort ticksSinceDash;
        private ushort ticksSinceParry;

        public TankSimulationState(Vector2 position, float hullRotation, Vector2 velocity, float turretRotation, TankAction performedAction, PlayerState playerState, ushort ticksSinceFire, ushort ticksSinceDash, ushort ticksSinceParry)
        {
            this.position = position;
            this.hullRotation = hullRotation;
            this.velocity = velocity;
            this.turretRotation = turretRotation;
            this.performedAction = performedAction;
            this.playerState = playerState;
            this.ticksSinceFire = ticksSinceFire;
            this.ticksSinceDash = ticksSinceDash;
            this.ticksSinceParry = ticksSinceParry;
            m_timestamp = -1;
        }

        public static int SerializedSize =>
                    FastBufferWriter.GetWriteSize(Vector2.one) + // position
                    sizeof(float) + // hullRotation
                    FastBufferWriter.GetWriteSize(Vector2.one) + // velocity
                    sizeof(float) + // turretRotation
                    sizeof(TankAction) + // performed
                    sizeof(PlayerState) + // PlayerState
                    sizeof(ushort) + // ticksSinceFire
                    sizeof(ushort) + // ticksSinceDash
                    sizeof(ushort); // ticksSinceParry

        internal void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref position);
            serializer.SerializeValue(ref hullRotation);
            serializer.SerializeValue(ref velocity);
            serializer.SerializeValue(ref turretRotation);
            serializer.SerializeValue(ref performedAction);
            serializer.SerializeValue(ref ticksSinceFire);
            serializer.SerializeValue(ref ticksSinceDash);
            serializer.SerializeValue(ref ticksSinceParry);
        }

    }
}