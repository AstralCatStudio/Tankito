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
        //public ulong ownerID;
    }

    public class BulletCannon : NetworkBehaviour
    {
        public BulletProperties m_bulletProperties;
        [SerializeField]
        private List<BulletModifier> m_bulletModifiers = new List<BulletModifier>();
        
        // Esto deberia ser comun para todos los bullets no deberia ser algo de cada disparador
        
        public int ReloadTicks => (int)(m_reloadTime/SimClock.SimDeltaTime);
        float m_reloadTime = 1;
        [SerializeField]
        float m_baseReloadTime = 0.5f;
        List<GameObject> bulletsShot = new List<GameObject>();
        private List<Vector2> BulletDirections = new List<Vector2>();
        public int baseBulletAmount;
        public int m_bulletAmount;
        int spawnTickTime = 0;
        [SerializeField]
        float m_shootRadius, m_shootSpreadAngle, m_scatterAngle;
        public BulletModifier bulletSpriteModifier;
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
            const float minimunLifetime = 1f;

            BulletDirections.Clear();
            BulletDirections.Add(new Vector2(1,0)) ;
            m_bulletAmount = baseBulletAmount;
            m_bulletProperties = BulletCannonRegistry.Instance.BaseProperties;
            m_reloadTime = m_baseReloadTime;
            int maxBulletSpritePriority = 0;
            foreach (BulletModifier modifier in m_bulletModifiers)
            {
                m_bulletProperties.velocity *= modifier.bulletStatsModifier.speedMultiplier;
                
                m_bulletProperties.scaleMultiplier *= modifier.bulletStatsModifier.sizeMultiplier;
                
                m_bulletProperties.acceleration += modifier.bulletStatsModifier.accelerationAdded;
                m_bulletProperties.bouncesTotal += modifier.bulletStatsModifier.BouncesAdded;
                m_bulletProperties.lifetimeTotal += modifier.bulletStatsModifier.lifeTimeAdded;
                m_bulletProperties.lifetimeTotal = (m_bulletProperties.lifetimeTotal < minimunLifetime) ? minimunLifetime : m_bulletProperties.lifetimeTotal;
                BulletDirections.AddRange(modifier.bulletStatsModifier.BulletDirections);
                m_bulletAmount += modifier.bulletStatsModifier.amountAdded;
                m_bulletAmount *= modifier.bulletStatsModifier.amountMultiplier;
                m_reloadTime += modifier.bulletStatsModifier.reloadTimeAdded;
                if (modifier.bulletSpritePriority > maxBulletSpritePriority)
                {
                    maxBulletSpritePriority = modifier.bulletSpritePriority;
                    if (modifier.bulletSprite != null)
                    {
                        bulletSpriteModifier = modifier;
                    }
                }
            }
            if (m_bulletProperties.scaleMultiplier.x < 0.1f)
            {
                m_bulletProperties.scaleMultiplier = new Vector2(0.1f, 0.1f);
            }
            else if (m_bulletProperties.scaleMultiplier.x > 3)
            {
                m_bulletProperties.scaleMultiplier = new Vector2(3, 3);
            }
        }

        public void Shoot(Vector2 originPosition, Vector2 aimVector, int tick)
        {
            float baseAngle = Mathf.Atan2(aimVector.y, aimVector.x) ;
            Shoot(originPosition, baseAngle, tick);
        }

        public void Shoot(Vector2 originPosition, float aimAngle, int tick)
        {
            int spawnN = 0;
            for (int i = 0; i < BulletDirections.Count; i++)
            {
                float newAngle = Mathf.Atan2(BulletDirections[i].y, BulletDirections[i].x)+ aimAngle;
                Vector2 newAimVector = new Vector2(Mathf.Cos(newAngle), Mathf.Sin(newAngle));
                for (int j = 0; j < m_bulletAmount; j++)
                {
                    float angle = newAngle + (-m_shootSpreadAngle/2 + m_shootSpreadAngle/(m_bulletAmount+1)*(j+1))*Mathf.Deg2Rad;
                    //Debug.Log("bala " + (j+1) + ": angulo " + angle);
                    Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    ShootBullet(originPosition + m_shootRadius * newAimVector, direction, spawnN, tick);
                    spawnN++;
                }
            }
        }

        void ShootBullet(Vector2 position, Vector2 direction, int spawnN, int tick)
        {
            m_bulletProperties.direction = direction;
            m_bulletProperties.startingPosition = position;
            m_bulletProperties.spawnTickTime = tick; // NO se puede usar SimClock.TickCounter porque no funciona durante input replay/Rollback
            var newBullet = BulletPool.Instance.Get(position, OwnerClientId, tick, spawnN);
        }

        public void ShootBulletFromBullet(Vector2 position, Vector2 direction, int spawnN, int tick, ulong originalBulletId)
        {
            m_bulletProperties.direction = direction;
            m_bulletProperties.startingPosition = position;
            m_bulletProperties.spawnTickTime = tick; // NO se puede usar SimClock.TickCounter porque no funciona durante input replay/Rollback
            var newBullet = BulletPool.Instance.Get(position, originalBulletId, tick, spawnN);
        }
        [ContextMenu("TestSpawning")]
        void TestSpawning()
        {
            ShootBullet(Vector2.zero, Vector2.zero, 0, 0);
        }
    }
}
