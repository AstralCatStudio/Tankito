using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Tankito.Netcode;
using UnityEngine.UIElements;

namespace Tankito {

    [System.Serializable]
    public struct BulletProperties : INetworkSerializable{ 
    
        public Vector2 scaleMultiplier;
        public Vector2 startingPosition;
        public float velocity;
        public float acceleration;
        public Vector2 direction;
        public float rotationSpeed;
        public int bouncesTotal;
        public float lifetimeTotal;

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
        protected BulletProperties m_properties;
        protected List<Modifier> m_modifierList = new List<Modifier>();
        public ulong m_shooterID;
        protected int m_bouncesLeft = 0;
        protected float m_lifetime = 0; // Life Time counter
        public Action OnSpawn = () => { }, OnFly = () => { }, OnHit = () => { }, OnBounce = () => { }, OnDetonate = () => { };


        public override void OnNetworkSpawn()
        {
            Debug.Log($"{this} properties: {m_properties}");
        }
        public override void OnNetworkDespawn()
        {
            ResetBulletData();
        }

        private void Awake()
        {
            
            foreach (var modifier in m_modifierList)
            {
                //modifier.ConnectModifier(this);
            }
        }

        public virtual void Init()
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
            m_shooterID = default;
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