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
        private List<Vector2> BulletDirections = new List<Vector2>();
        public int baseBulletAmount;
        public int m_bulletAmount;
        int spawnTickTime = 0;
        [SerializeField]
        float m_shootRadius, m_shootSpreadAngle, m_scatterAngle;
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
            BulletDirections.Clear();
            BulletDirections.Add(new Vector2(0,1)) ;
            m_bulletAmount = baseBulletAmount;
            m_bulletProperties = BulletCannonRegistry.Instance.BaseProperties;
            foreach (BulletModifier modifier in m_bulletModifiers)
            {
                m_bulletProperties.velocity *= modifier.bulletStatsModifier.speedMultiplier;
                m_bulletProperties.scaleMultiplier *= modifier.bulletStatsModifier.sizeMultiplier;
                m_bulletProperties.acceleration += modifier.bulletStatsModifier.accelerationAdded;
                m_bulletProperties.bouncesTotal += modifier.bulletStatsModifier.BouncesAdded;
                m_bulletProperties.lifetimeTotal += modifier.bulletStatsModifier.lifeTimeAdded;
                BulletDirections.AddRange(modifier.bulletStatsModifier.BulletDirections);
                m_bulletAmount += modifier.bulletStatsModifier.amountAdded;
                m_bulletAmount *= modifier.bulletStatsModifier.amountMultiplier;
            }
        }

        public void Shoot(Vector2 aimVector)
        {
            if (timer >= interval)
            {
                timer = 0;
                float baseAngle = Mathf.Atan2(aimVector.y, aimVector.x) ;
                for (int i = 0; i < BulletDirections.Count; i++)
                {
                    float newAngle = Mathf.Atan2(BulletDirections[i].y, BulletDirections[i].x)+ baseAngle;
                    for (int j = 0; j < m_bulletAmount; j++)
                    {
                        float angle = newAngle - (m_shootSpreadAngle / 2 + (m_shootSpreadAngle / (m_bulletAmount + 1)) * (j+1))*Mathf.Deg2Rad;
                        Debug.Log("bala "+(j+1) + ": angulo " + angle);
                        Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                        ShootBullet(m_shootRadius* direction, direction, j*i);
                    }
                }
            }
        }

        void ShootBullet(Vector2 position, Vector2 direction, int spawnN)
        {
            
            m_bulletProperties.direction = direction;
            m_bulletProperties.startingPosition = transform.position;
            m_bulletProperties.spawnTickTime = SimClock.TickCounter;
            Instantiate<GameObject>(BulletCannonRegistry.Instance.m_bulletPrefab);
            //var newBullet = BulletPool.Instance.Get(position, direction, OwnerClientId, SimClock.TickCounter, spawnN);
            //newBullet.AddToSim();
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
