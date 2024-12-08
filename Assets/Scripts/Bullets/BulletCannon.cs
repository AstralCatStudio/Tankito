using System;
using System.Collections;
using System.Collections.Generic;
using Tankito.Netcode;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

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
        [SerializeField]
        private List<BulletModifier> m_bulletModifiers = new List<BulletModifier>();
        
        // Esto deberia ser comun para todos los bullets no deberia ser algo de cada disparador
        
        float interval = 1;
        [SerializeField]
        float baseInterval = 0.5f;
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
            RoundManager.Instance.OnPreRoundStart += ApplyModifierProperties;
        }

        private void OnDisable()
        {
            RoundManager.Instance.OnPreRoundStart -= ApplyModifierProperties;
        }

        private void Start()
        {
            BulletCannonRegistry.Instance[OwnerClientId] = this;
            ApplyModifierProperties();
        }

        public void ApplyModifierProperties(int nRound = 0)
        {
            BulletDirections.Clear();
            BulletDirections.Add(new Vector2(1,0)) ;
            m_bulletAmount = baseBulletAmount;
            m_bulletProperties = BulletCannonRegistry.Instance.BaseProperties;
            interval = baseInterval;
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
                interval += modifier.bulletStatsModifier.reloadTimeAdded;
            }
        }

        public void Shoot(Vector2 aimVector)
        {
            if (timer >= interval)
            {
                int spawnN = 0;
                timer = 0;
                float baseAngle = Mathf.Atan2(aimVector.y, aimVector.x) ;
                for (int i = 0; i < BulletDirections.Count; i++)
                {
                    float newAngle = Mathf.Atan2(BulletDirections[i].y, BulletDirections[i].x)+ baseAngle;
                    Vector2 newAimVector = new Vector2(Mathf.Cos(newAngle), Mathf.Sin(newAngle));
                    for (int j = 0; j < m_bulletAmount; j++)
                    {
                        float angle = newAngle + (-m_shootSpreadAngle/2 + m_shootSpreadAngle/(m_bulletAmount+1)*(j+1))*Mathf.Deg2Rad;
                        //Debug.Log("bala " + (j+1) + ": angulo " + angle);
                        Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                        ShootBullet((Vector2)transform.parent.parent.parent.position + m_shootRadius * newAimVector, direction, spawnN);
                        spawnN++;
                    }
                }
            }
        }

        void ShootBullet(Vector2 position, Vector2 direction, int spawnN)
        {
            
            m_bulletProperties.direction = direction;
            m_bulletProperties.startingPosition = position;
            m_bulletProperties.spawnTickTime = SimClock.TickCounter;
            var newBullet = BulletPool.Instance.Get(position, direction, OwnerClientId, SimClock.TickCounter, spawnN);
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
