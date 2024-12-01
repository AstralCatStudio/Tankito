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

        // VAMOS A ABANDONAR POR EL MOMENTO EL ENCODING RELATIVO AL NUMERO DE TIMESTAMP DEL SNAPSHOT PORQUE ES MAS DIFICIL DE LO QUE PARECE HACERLO BIEN,
        // y el beneficio tan solo es ahorar unos pocos bytes (int vs ushort) en los snapshots realmente que probablemente se ahorren mejor con algun algoritmo de compresion
        // como zlib o z standard
        /*
        /// <summary>
        /// Used to calculate absolute ticks (fire, parry, dash).
        /// <para/>NOT MEANT TO BE SERIALIZED DIRECTLY. Use
        /// <see cref="Timestamp"/> instead. 
        /// </summary>
        private int m_timestamp;
        public int Timestamp { private get => (m_timestamp >= 0) ? m_timestamp : throw new InvalidOperationException("m_timestamp is uninitialized");
                                        set => m_timestamp = value; }
        */

        public int LastFireTick { get => lastFireTick; }
        public int LastDashTick { get => lastDashTick; }
        public int LastParryTick { get => lastParryTick; }

        public PlayerState PlayerState { get => playerState; }

        private Vector2 position;
        private float hullRotation;
        private Vector2 velocity;
        private float turretRotation;
        private TankAction performedAction;
        private PlayerState playerState;
        private int lastFireTick;
        private int lastDashTick;
        private int lastParryTick;

        public TankSimulationState(Vector2 position, float hullRotation, Vector2 velocity, float turretRotation, TankAction performedAction, PlayerState playerState, int lastFireTick, int lastDashTick, int lastParryTick)
        {
            this.position = position;
            this.hullRotation = hullRotation;
            this.velocity = velocity;
            this.turretRotation = turretRotation;
            this.performedAction = performedAction;
            this.playerState = playerState;
            this.lastFireTick = lastFireTick;
            this.lastDashTick = lastDashTick;
            this.lastParryTick = lastParryTick;
        }

        public static int SerializedSize =>
                    FastBufferWriter.GetWriteSize(Vector2.one) + // position
                    sizeof(float) + // hullRotation
                    FastBufferWriter.GetWriteSize(Vector2.one) + // velocity
                    sizeof(float) + // turretRotation
                    sizeof(TankAction) + // performed
                    sizeof(PlayerState) + // PlayerState
                    sizeof(int) + // lastFireTick
                    sizeof(int) + // lastDashTick
                    sizeof(int); // lastParryTick

        internal void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref position);
            serializer.SerializeValue(ref hullRotation);
            serializer.SerializeValue(ref velocity);
            serializer.SerializeValue(ref turretRotation);
            serializer.SerializeValue(ref performedAction);
            serializer.SerializeValue(ref playerState);
            serializer.SerializeValue(ref lastFireTick);
            serializer.SerializeValue(ref lastDashTick);
            serializer.SerializeValue(ref lastParryTick);
        }

        public override string ToString()
        {
            return "[ " +
                    $"position: {position} | " +
                    $"hullRotation: {hullRotation} | " +
                    $"velocity: {velocity} | " +
                    $"turretRotation: {turretRotation} | " +
                    $"performedAction: {performedAction} | " +
                    $"playerState: {playerState} | " +
                    $"lastFireTick: {lastFireTick} | " +
                    $"lastDashTick: {lastDashTick} | " +
                    $"lastParryTick: {lastParryTick}" + 
                    " ]";
        }
    }
}