using System;
using UnityEngine;

namespace Tankito.Netcode.Simulation
{
    public class BulletSimulationObject : ASimulationObject
    {
        [SerializeField] private Rigidbody2D m_rigidbody;
        [SerializeField] private ABullet m_bullet;
        public int SpawnTick { get => m_spawnTick; }
        private int m_spawnTick;

        void Start()
        {
            if (m_bullet == null)
            {
                m_bullet = GetComponent<ABullet>();
                if (m_bullet == null) Debug.LogWarning("BulletSimulationObject could not find associated ABullet component!");
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            m_spawnTick = SimClock.TickCounter;
        }
        public override ISimulationState GetSimState()
        {
            return new BulletSimulationState(
                m_rigidbody.position,
                m_rigidbody.rotation,
                m_rigidbody.velocity,
                m_bullet.m_lifetime
            );
        }

        public override void SetSimState(in ISimulationState state)
        {
            if (state is BulletSimulationState bulletState)
            {
                m_rigidbody.position = bulletState.Position;
                m_rigidbody.rotation = bulletState.Rotation;
                m_rigidbody.velocity = bulletState.Velocity;
                m_bullet.m_lifetime = bulletState.LifeTime;
            }
            else
            {
                throw new ArgumentException("Invalid state type");
            }
        }
    }
}