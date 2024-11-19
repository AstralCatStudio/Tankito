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

        private Vector2 position;
        private float rotation;
        private Vector2 velocity;
        private float lifeTime; // In seconds

        public const int MAX_SERIALIZED_SIZE = sizeof(float)*2 + sizeof(float)*2*2;

        public BulletSimulationState(Vector2 position, float rotation, Vector2 velocity, float lifeTime)
        {
            this.position = position;
            this.rotation = rotation;
            this.velocity = velocity;
            this.lifeTime = lifeTime;
        }

        internal void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref position);
            serializer.SerializeValue(ref rotation);
            serializer.SerializeValue(ref velocity);
            serializer.SerializeValue(ref lifeTime);
        }
    }
}


