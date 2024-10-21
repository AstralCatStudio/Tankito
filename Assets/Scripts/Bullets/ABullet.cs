using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Tankito.Netcode;

namespace Tankito {

    [System.Serializable]
    public struct BulletProperties
    {
        public Vector2 ScaleMultiplier;
        public Vector2 startingPosition;
        public float velocity;
        public float acceleration;
        public Vector2 direction;
        public float rotationSpeed;
        public int bouncesTotal;
        public float lifetimeTotal;
    }
    public abstract class ABullet : NetworkBehaviour
    {
        [SerializeField]
        protected BulletProperties m_properties;
        protected List<Modifier> m_modifierList = new List<Modifier>();
        public ulong m_ownerID;
        protected int m_bouncesLeft = 0;
        protected float m_lifetime;
        public Action OnSpawn = () => { }, OnFly = () => { }, OnHit = () => { }, OnBounce = () => { }, OnDetonate = () => { };

        public override void OnNetworkDespawn()
        {
            ResetBulletData();
        }

        private void Awake()
        {
            
            foreach (var modifier in m_modifierList)
            {
                modifier.ConnectModifier(this);
            }
        }

        void Start()
        {
            OnSpawn.Invoke();
        }
        
        void Update()
        {
            OnFly.Invoke();
        }

        public void SetProperties(BulletProperties newProperties)
        {
            m_properties = newProperties;
        }

        protected void ResetBulletData()
        {
            m_properties = default;
            m_ownerID = default;
            m_bouncesLeft = 0;
            m_lifetime = 0;
            m_modifierList.Clear();
            OnSpawn = () => {};
            OnFly = () => {};
            OnHit = () => {};
            OnBounce = () => {};
            OnDetonate = () => {};
        }

        
    }
}