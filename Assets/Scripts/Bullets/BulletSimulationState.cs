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
        public float Rotation { get => rotation; }
        public Vector2 Velocity { get => velocity; }
        public float LifeTime { get => lifeTime; }
        public int BouncesLeft { get => bouncesLeft; }
        public ulong OwnerId { get => ownerId; }

        private Vector2 position;
        private float rotation;
        private Vector2 velocity;
        private float lifeTime; // In seconds
        private int bouncesLeft;
        private ulong ownerId;

        public BulletSimulationState(Vector2 position, float rotation, Vector2 velocity, float lifeTime, int bouncesLeft, ulong ownerId)
        {
            this.position = position;
            this.rotation = rotation;
            this.velocity = velocity;
            this.lifeTime = lifeTime;
            this.bouncesLeft = bouncesLeft;
            this.ownerId = ownerId;
        }

        public static int SerializedSize =>
                    FastBufferWriter.GetWriteSize(Vector2.one) + // position
                    sizeof(float) + // rotation
                    FastBufferWriter.GetWriteSize(Vector2.one) + // velocity
                    sizeof(float) + // lifeTime
                    sizeof(int) + // bouncesLeft
                    sizeof(ulong); // ownerId
                    

        internal void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref position);
            serializer.SerializeValue(ref rotation);
            serializer.SerializeValue(ref velocity);
            serializer.SerializeValue(ref lifeTime);
            serializer.SerializeValue(ref bouncesLeft);
            serializer.SerializeValue(ref ownerId);
        }

        public override string ToString()
        {
            return "[ " +
                    "position: " + position + " | " +
                    "rotation: " +  rotation + " | " +
                    "velocity: " + velocity + " | " +
                    "lifeTime: " + bouncesLeft + " | " +
                    "ownerId: " + ownerId + " | ";
        }
    }
}


