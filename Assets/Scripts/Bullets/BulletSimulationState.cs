using System;
using System.Collections;
using System.Collections.Generic;
using Tankito.Netcode.Simulation;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

namespace Tankito.Netcode.Simulation
{
    public struct BulletSimulationState : ISimulationState
    {

        public Vector2 Position { get => position; }
        public Vector2 Velocity { get => velocity; }
        public float Lifetime { get => lifeTime; }
        public int BouncesLeft { get => bouncesLeft; }
        public ulong OwnerId { get => ownerId; }
        public ulong LastShooterObjId { get => lastShooterObjId; }

        private Vector2 position;
        private Vector2 velocity;
        private float lifeTime; // In seconds
        private int bouncesLeft;
        private ulong ownerId;
        private ulong lastShooterObjId;

        public BulletSimulationState(Vector2 position, Vector2 velocity, float lifeTime, int bouncesLeft, ulong ownerId, ulong lastShooterObjId)
        {
            this.position = position;
            this.velocity = velocity;
            this.lifeTime = lifeTime;
            this.bouncesLeft = bouncesLeft;
            this.ownerId = ownerId;
            this.lastShooterObjId = lastShooterObjId;
        }

        public static int SerializedSize =>
                    FastBufferWriter.GetWriteSize(Vector2.one) + // position
                    FastBufferWriter.GetWriteSize(Vector2.one) + // velocity
                    sizeof(float) + // lifeTime
                    sizeof(int) + // bouncesLeft
                    sizeof(ulong) + // ownerId
                    sizeof(ulong); // lastShooterObjId


        internal void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref position);
            serializer.SerializeValue(ref velocity);
            serializer.SerializeValue(ref lifeTime);
            serializer.SerializeValue(ref bouncesLeft);
            serializer.SerializeValue(ref ownerId);
            serializer.SerializeValue(ref lastShooterObjId);
        }

        public override string ToString()
        {
            return "[ " +
                    "position: " + position + " | " +
                    "velocity: " + velocity + " | " +
                    "lifeTime: " + lifeTime + " | " +
                    "bouncesLeft: " + bouncesLeft + " | " +
                    "ownerId: " + ownerId + " | " +
                    "lastShooterObjId: " + lastShooterObjId + " | ";
        }
    }
}


