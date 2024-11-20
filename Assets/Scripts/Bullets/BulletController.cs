using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Tankito.Netcode;
using UnityEngine.UIElements;
using Tankito.Netcode.Simulation;
using UnityEditor.PackageManager;

namespace Tankito {

    
    public class BulletController : NetworkBehaviour
    {
        protected int m_bouncesLeft = 0;
        public float LifeTime { get => m_lifetime; }
        public float m_lifetime = 0; // Life Time counter
        protected Vector2 lastCollisionNormal = Vector2.zero;
        private Rigidbody2D m_rb;
        public Action<BulletController> OnSpawn = (ABullet) => { }, OnFly = (ABullet) => { }, OnHit = (ABullet) => { }, OnBounce = (ABullet) => { }, OnDetonate = (ABullet) => { };
        private void Awake()
        {
            m_rb = GetComponent<Rigidbody2D>();
        }
        private void OnEnable()
        {
            
            GetComponent<BulletSimulationObject>().OnComputeKinematics += MoveBullet;
        }
        private void OnDisable()
        {
            GetComponent<BulletSimulationObject>().OnComputeKinematics -= MoveBullet;
        }
        void MoveBullet(float deltaTime)
        {
            transform.rotation = Quaternion.LookRotation(new Vector3(0, 0, 1), m_rb.velocity.normalized);
            m_rb.velocity += (BulletCannonRegistry.Instance[OwnerClientId].Properties.acceleration != 0f) ? BulletCannonRegistry.Instance[OwnerClientId].Properties.acceleration * m_rb.velocity.normalized : Vector2.zero;
            m_lifetime += Time.deltaTime;
            OnFly.Invoke(this);
            if (IsServer)
            {
                if (m_lifetime >= BulletCannonRegistry.Instance[OwnerClientId].Properties.lifetimeTotal)
                {
                    Debug.Log($"lifetime: {m_lifetime}/{BulletCannonRegistry.Instance[OwnerClientId].Properties.lifetimeTotal}");
                    Detonate();
                }
            }
        }
        public override void OnNetworkSpawn()
        {
            m_bouncesLeft = BulletCannonRegistry.Instance[OwnerClientId].Properties.bouncesTotal;
            m_rb.velocity = BulletCannonRegistry.Instance[OwnerClientId].Properties.velocity * BulletCannonRegistry.Instance[OwnerClientId].Properties.direction.normalized;
            transform.position = BulletCannonRegistry.Instance[OwnerClientId].transform.position;
            foreach (var modifier in Tankito.BulletCannonRegistry.Instance[OwnerClientId].Modifiers)
            {
                modifier.BindBulletEvents(this);
            }
            OnSpawn.Invoke(this);
        }
        
        public override void OnNetworkDespawn()
        {
            ResetBulletData();
        }

        protected void ResetBulletData()
        {
            m_bouncesLeft = 0;
            m_lifetime = 0;
            OnSpawn = (ABullet) => {};
            OnFly = (ABullet) => {};
            OnHit = (ABullet) => {};
            OnBounce = (ABullet) => {};
            OnDetonate = (ABullet) => {};
        }
        public void Detonate()
        {
            OnDetonate.Invoke(this);
            if (IsServer)
            {
                var networkObject = gameObject.GetComponent<NetworkObject>();
                networkObject.Despawn();
            }
            else
            {
                OnDetonate.Invoke(this);
                m_rb.velocity = Vector2.zero;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            lastCollisionNormal = collision.GetContact(0).normal;
            switch (collision.gameObject.tag)
            {
                case "NormalWall":
                    if (m_bouncesLeft <= 0)
                    {
                        Detonate();
                    }
                    else
                    {
                        m_bouncesLeft--;
                    }
                    break;
                case "BouncyWall":

                    break;

                case "Player":
                    if (collision.gameObject.GetComponent<NetworkObject>().OwnerClientId == OwnerClientId && m_lifetime < 0.03f)
                    {
                        //Debug.Log("Ignoing firing self collision");
                        //Detonate();
                    }
                    else
                    {
                        Detonate();
                    }
                    break;

                default:
                    Detonate();
                    break;
            }
        }

    }
}