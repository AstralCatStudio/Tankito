using System;
using UnityEngine;

namespace Tankito.Netcode.Simulation
{
    public class BulletSimulationObject : ASimulationObject
    {
        [SerializeField] private Rigidbody2D m_rigidbody;
        [SerializeField] private ABullet m_bullet;

        void Start()
        {
            if (m_bullet == null)
            {
                m_bullet = GetComponent<ABullet>();
                if (m_bullet == null) Debug.LogWarning("BulletSimulationObject could not find associated ABullet component!");
            }
        }

        public override ISimulationState GetSimState()
        {
            return new BulletSimulationState(
                m_rigidbody.position,
                m_rigidbody.rotation,
                m_rigidbody.velocity,
                m_bullet.LifeTime
            );
        }

        public override void SetSimState(in ISimulationState state)
        {
            throw new NotImplementedException();
        }
    }
}