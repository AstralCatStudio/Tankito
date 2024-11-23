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

    
    public class BulletController : MonoBehaviour
    {
        ulong ownerId = 0;
        bool simulated = false;
        public int m_bouncesLeft = 0;
        public float LifeTime { get => m_lifetime; }
        public float m_lifetime = 0; // Life Time counter
        protected Vector2 lastCollisionNormal = Vector2.zero;
        private Rigidbody2D m_rb;
        public float selfCollisionTreshold = 0.1f;
        public Action<BulletController> OnSpawn = (ABullet) => { }, OnFly = (ABullet) => { }, OnHit = (ABullet) => { }, OnBounce = (ABullet) => { }, OnDetonate = (ABullet) => { };
        private void Awake()
        {
            m_rb = GetComponent<Rigidbody2D>();
        }
        
        private void OnEnable()
        {
            
            GetComponent<BulletSimulationObject>().OnComputeKinematics += MoveBullet;

            transform.position = BulletCannonRegistry.Instance[ownerId].transform.position;
            m_bouncesLeft = BulletCannonRegistry.Instance[ownerId].Properties.bouncesTotal;
            m_rb.velocity = BulletCannonRegistry.Instance[ownerId].Properties.velocity * BulletCannonRegistry.Instance[ownerId].Properties.direction.normalized;
            transform.rotation = Quaternion.LookRotation(new Vector3(0, 0, 1), m_rb.velocity.normalized);
            foreach (var modifier in Tankito.BulletCannonRegistry.Instance[ownerId].Modifiers)
            {
                modifier.BindBulletEvents(this);
            }


            OnSpawn.Invoke(this);
        }

        private void OnDisable()
        {
            GetComponent<BulletSimulationObject>().OnComputeKinematics -= MoveBullet;
            
            ResetBulletData();
        }

        private void Update()
        {
            transform.rotation = Quaternion.LookRotation(new Vector3(0, 0, 1), m_rb.velocity.normalized);
            MoveBullet(Time.deltaTime);//borrar
        }

        void MoveBullet(float deltaTime)
        {
            if(m_lifetime >= selfCollisionTreshold)
            {
                gameObject.layer = 0;
            }
            m_rb.velocity += (BulletCannonRegistry.Instance[ownerId].Properties.acceleration != 0f) ? BulletCannonRegistry.Instance[ownerId].Properties.acceleration * m_rb.velocity.normalized : Vector2.zero;
            m_lifetime += deltaTime;
            OnFly.Invoke(this);
            if (m_lifetime >= BulletCannonRegistry.Instance[ownerId].Properties.lifetimeTotal)
            {
                Debug.Log($"lifetime: {m_lifetime}/{BulletCannonRegistry.Instance[ownerId].Properties.lifetimeTotal}");
                Detonate();
            }
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
            if (NetworkManager.Singleton.IsServer)
            {
                BulletSimulationObject bulletSimulation = GetComponent<BulletSimulationObject>();
                Destroy(gameObject);
                //BulletPool.Instance.Release(bulletSimulation);
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
                    if (collision.gameObject.GetComponent<NetworkObject>().OwnerClientId == ownerId && m_lifetime < 0.03f)
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