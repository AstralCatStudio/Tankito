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
        public float lifetimetotal;
    }
    public abstract class ABullet : MonoBehaviour
    {
        public BulletProperties bulletProperties;
        protected List<Modifier> modifiersList = new List<Modifier>();
        protected ulong m_ownerID;
        protected int bouncesLeft=0;
        protected float lifetimeLeft;
        public Action OnSpawn = () => { }, OnFly = () => { }, OnHit = () => { }, OnBounce = () => { }, OnDetonate = () => { };
        private void Awake()
        {
            foreach (var modifier in modifiersList)
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
    }
}