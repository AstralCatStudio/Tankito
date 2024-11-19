using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Tankito.Netcode;
using UnityEngine.UIElements;

namespace Tankito {

    [System.Serializable]
    public struct BulletProperties
    {
        public Vector2 scaleMultiplier;
        public Vector2 startingPosition;
        public float velocity;
        public float acceleration;
        public Vector2 direction;
        public float rotationSpeed;
        public int bouncesTotal;
        public float lifetimeTotal;
        public int spawnTickTime;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref scaleMultiplier);
            serializer.SerializeValue(ref startingPosition);
            serializer.SerializeValue(ref velocity);
            serializer.SerializeValue(ref acceleration);
            serializer.SerializeValue(ref direction);
            serializer.SerializeValue(ref rotationSpeed);
            serializer.SerializeValue(ref bouncesTotal);
            serializer.SerializeValue(ref lifetimeTotal);
        }

        public override string ToString()
        {
            return $"| scaleMultiplier-{scaleMultiplier} | startingPosition-{startingPosition} | velocity-{velocity} | acceleration-{acceleration} | direction-{direction} | rotationSpeed-{rotationSpeed} | bouncesTotal-{bouncesTotal} | lifetimeTotal-{lifetimeTotal} |";
        }
    }
    public abstract class ABullet : NetworkBehaviour
    {
        [SerializeField]
        protected BulletProperties m_bulletProperties;
        public ulong m_shooterID;
        protected int m_bouncesLeft = 0;
        public float LifeTime { get => m_lifetime; }
        public float m_lifetime = 0; // Life Time counter
        protected Vector2 lastCollisionNormal = Vector2.zero;
        
        public Action<ABullet> OnSpawn = (ABullet) => { }, OnFly = (ABullet) => { }, OnHit = (ABullet) => { }, OnBounce = (ABullet) => { }, OnDetonate = (ABullet) => { };
        

        public override void OnNetworkSpawn()
        {
            //Debug.Log($"{this} properties: {m_properties}");
        }
        
        public override void OnNetworkDespawn()
        {
            ResetBulletData();
        }

        private void Awake()
        {
            
            foreach (var modifier in Tankito.BulletCannonRegistry.Instance[m_shooterID].Modifiers)
            {
                modifier.BindBulletEvents(this);
            }
        }

        public virtual void Init()
        {
            OnSpawn.Invoke(this);
        }
        
        void Update()
        {
            OnFly.Invoke(this);
        }

        protected void ResetBulletData()
        {
            BulletCannonRegistry = default;
            m_shooterID = default;
            m_bouncesLeft = 0;
            m_lifetime = 0;
            OnSpawn = (ABullet) => {};
            OnFly = (ABullet) => {};
            OnHit = (ABullet) => {};
            OnBounce = (ABullet) => {};
            OnDetonate = (ABullet) => {};
        }

        
    }
}