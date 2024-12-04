using System;
using UnityEngine;

namespace Tankito.Netcode.Simulation
{
    public class BulletSimulationObject : ASimulationObject
    {
        [SerializeField] private Rigidbody2D m_rigidbody;
        [SerializeField] private BulletController m_bulletController;
        public int SpawnTick { get => m_spawnTick; }
        private int m_spawnTick;
        private ulong m_ownerId;
        public ulong OwnerId => m_ownerId;

        public override SimulationObjectType SimObjType => SimulationObjectType.Bullet;

        void Start()
        {
            if (m_bulletController == null)
            {
                m_bulletController = GetComponent<BulletController>();
                if (m_bulletController == null) Debug.LogWarning("BulletSimulationObject could not find associated ABullet component!");
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
                m_rigidbody.velocity,
                m_bulletController.LifeTime,
                m_bulletController.m_bouncesLeft,
                OwnerId
            );
        }

        public override void SetSimState(in ISimulationState state)
        {
            if (state is BulletSimulationState bulletState)
            {
                Debug.Log($"SetSimState to: pos({bulletState.Position}), vel({bulletState.Velocity}), LifeTime({bulletState.LifeTime}), BouncesLeft({bulletState.BouncesLeft})");
                transform.position = bulletState.Position;
                m_rigidbody.velocity = bulletState.Velocity;
                m_bulletController.LifeTime = bulletState.LifeTime;
                m_bulletController.m_bouncesLeft = bulletState.BouncesLeft;
            }
            else
            {
                throw new ArgumentException("Invalid state type");
            }
        }


    }
}