using System;
using Unity.Netcode;
using UnityEngine;

namespace Tankito.Netcode.Simulation
{
    public class BulletSimulationObject : ASimulationObject
    {
        [SerializeField] private Rigidbody2D m_rigidbody;
        [SerializeField] private BulletController m_bullet;
        public int SpawnTick { get => m_spawnTick; }
        private int m_spawnTick;
        private ulong m_ownerId;
        public ulong OwnerId => m_ownerId;

        public override SimulationObjectType SimObjType => SimulationObjectType.Bullet;

        void Start()
        {
            if (m_bullet == null)
            {
                m_bullet = GetComponent<BulletController>();
                if (m_bullet == null) Debug.LogWarning("BulletSimulationObject could not find associated ABullet component!");
            }
        }
        
        public void SetOwner(ulong ownerId)
        {
            m_ownerId = ownerId;
        }

        /// <summary>
        /// Automatically removes the simulation object from the local <see cref="Simulation.NetSimulationManager.Instance"/>.
        /// And also releases the bullet to the local <see cref="BulletPool.Instance"/> 
        /// </summary>
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            BulletPool.Instance.Release(this);
        }


        public override ISimulationState GetSimState()
        {
            return new BulletSimulationState(
                m_rigidbody.position,
                m_rigidbody.rotation,
                m_rigidbody.velocity,
                m_bullet.m_lifetime,
                m_bullet.m_bouncesLeft,
                SimObjId,
                OwnerId
            );
        }

        public override void SetSimState(in ISimulationState state)
        {
            if(!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
            if (state is BulletSimulationState bulletState)
            {
                m_rigidbody.position = bulletState.Position;
                m_rigidbody.rotation = bulletState.Rotation;
                m_rigidbody.velocity = bulletState.Velocity;
                m_bullet.m_lifetime = bulletState.LifeTime;
                m_bullet.m_bouncesLeft = bulletState.BouncesLeft;
            }
            else
            {
                throw new ArgumentException("Invalid state type");
            }
        }


    }
}