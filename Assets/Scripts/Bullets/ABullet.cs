using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

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
        protected List<Modifier> m_modifiersList = new List<Modifier>();
        protected ulong m_ownerID;
        protected int m_bouncesLeft=0;
        protected float m_lifetime;
        public Action OnSpawn = () => { }, OnFly = () => { }, OnHit = () => { }, OnBounce = () => { }, OnDetonate = () => { };
        private void Awake()
        {
            foreach (var modifier in m_modifiersList)
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
    }
}