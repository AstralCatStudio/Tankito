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
        BulletSimulationObject m_simObj;
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
            m_simObj = GetComponent<BulletSimulationObject>();
            m_rb = GetComponent<Rigidbody2D>();
        }

        public void InitializeProperties(bool triggerOnSpawnEvents = true)
        {           
            GetComponent<BulletSimulationObject>().OnComputeKinematics += MoveBullet;

            transform.position = BulletCannonRegistry.Instance[m_simObj.OwnerId].Properties.startingPosition;
            m_bouncesLeft = BulletCannonRegistry.Instance[m_simObj.OwnerId].Properties.bouncesTotal;
            m_rb.velocity = BulletCannonRegistry.Instance[m_simObj.OwnerId].Properties.velocity * BulletCannonRegistry.Instance[m_simObj.OwnerId].Properties.direction.normalized;
            transform.rotation = Quaternion.LookRotation(new Vector3(0, 0, 1), m_rb.velocity.normalized);
            foreach (var modifier in Tankito.BulletCannonRegistry.Instance[m_simObj.OwnerId].Modifiers)
            {
                modifier.BindBulletEvents(this);
            }

            if (triggerOnSpawnEvents) OnSpawn?.Invoke(this);
        }

        private void OnDisable()
        {
            GetComponent<BulletSimulationObject>().OnComputeKinematics -= MoveBullet;
            
            ResetBulletData();
        }

        private void Update()
        {
            transform.rotation = Quaternion.LookRotation(new Vector3(0, 0, 1), m_rb.velocity.normalized);
        }

        void MoveBullet(float deltaTime)
        {
            if (m_lifetime >= selfCollisionTreshold)
            {
                gameObject.layer = 0;
            }
            m_rb.velocity += (BulletCannonRegistry.Instance[m_simObj.OwnerId].Properties.acceleration != 0f) ? BulletCannonRegistry.Instance[m_simObj.OwnerId].Properties.acceleration * m_rb.velocity.normalized : Vector2.zero;
            m_lifetime += deltaTime;
            OnFly.Invoke(this);
            if (m_lifetime >= BulletCannonRegistry.Instance[m_simObj.OwnerId].Properties.lifetimeTotal)
            {
                Debug.Log($"lifetime: {m_lifetime}/{BulletCannonRegistry.Instance[m_simObj.OwnerId].Properties.lifetimeTotal}");
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
                BulletSimulationObject bulletSimObj = GetComponent<BulletSimulationObject>();
                ServerSimulationManager.Instance.QueueForRemoval(bulletSimObj.SimObjId);
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
                    if (collision.gameObject.GetComponent<NetworkObject>().OwnerClientId == m_simObj.OwnerId && m_lifetime < 0.03f)
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