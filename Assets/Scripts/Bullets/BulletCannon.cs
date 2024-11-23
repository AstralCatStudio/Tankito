using System;
using System.Collections;
using System.Collections.Generic;
using Tankito.Netcode;
using Unity.Netcode;
using UnityEngine;

namespace Tankito
{
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
        public ulong ownerID;
    }

    public class BulletCannon : NetworkBehaviour
    {
        public BulletProperties m_bulletProperties;
        private List<BulletModifier> m_bulletModifiers = new List<BulletModifier>();

        // Esto deberia ser comun para todos los bullets no deberia ser algo de cada disparador
        [SerializeField]
        float interval = 1;
        float timer = 0;
        List<GameObject> bulletsShot = new List<GameObject>();
        private List<Vector2> BulletDirection = new List<Vector2>();
        public int baseBulletAmount;
        int bulletAmount;
        int spawnTickTime = 0;
        //public Queue<GameObject> simulatedBullets = new Queue<GameObject>();
        public List<BulletModifier> Modifiers { get => m_bulletModifiers; }
        public BulletProperties Properties { get => m_bulletProperties; }

        private void OnEnable()
        {
            RoundManager.Instance.OnRoundStart += ApplyModifierProperties;
        }

        private void OnDisable()
        {
            RoundManager.Instance.OnRoundStart -= ApplyModifierProperties;
        }

        private void Start()
        {
            BulletCannonRegistry.Instance[OwnerClientId] = this;
            ApplyModifierProperties();
        }

        public void ApplyModifierProperties(int nRound = 0)
        {
            BulletDirection.Clear();
            BulletDirection.Add(transform.right);
            bulletAmount = baseBulletAmount;
            m_bulletProperties = BulletCannonRegistry.Instance.BaseProperties;
            foreach (BulletModifier modifier in m_bulletModifiers)
            {
                m_bulletProperties.velocity *= modifier.bulletStatsModifier.speedMultiplier;
                m_bulletProperties.scaleMultiplier *= modifier.bulletStatsModifier.sizeMultiplier;
                m_bulletProperties.acceleration += modifier.bulletStatsModifier.accelerationAdded;
                m_bulletProperties.bouncesTotal += modifier.bulletStatsModifier.BouncesAdded;
                m_bulletProperties.lifetimeTotal += modifier.bulletStatsModifier.lifeTimeAdded;
                BulletDirection.AddRange(modifier.bulletStatsModifier.BulletDirections);
                bulletAmount += modifier.bulletStatsModifier.amountAdded;
                bulletAmount *= modifier.bulletStatsModifier.amountMultiplier;
            }
        }

        public void Shoot(Vector2 aimVector)
        {
                if (timer >= interval)
                {
                    timer = 0;
                    //ShootBullet(, aimVector);
                }
        }

        void ShootBullet(Vector2 position, Vector2 direction, int spawnN)
        {
            //Vector2 direction;
            //float angle;
            //foreach (var item in BulletDirection)
            //{
            //    for (int i = 0; i < bulletAmount; i++)
            //    {
            //        if (bulletAmount % 2 == 0)
            //        {

            //        }
            //    }
            //}
            m_bulletProperties.direction = direction;
            m_bulletProperties.startingPosition = transform.position;
            m_bulletProperties.spawnTickTime = SimClock.TickCounter;
            
            var newBullet = BulletPool.Instance.Get(position, direction, OwnerClientId, SimClock.TickCounter, spawnN);
            newBullet.AddToSim();
        }

        void Update()
        {            
                timer += Time.deltaTime;
        }

        [ContextMenu("TestSpawning")]
        void TestSpawning()
        {
            ShootBullet(Vector2.zero, Vector2.zero, 0);
        }
    }
}
