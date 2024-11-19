using System;
using System.Collections;
using System.Collections.Generic;
using Tankito.Netcode;
using Unity.Netcode;
using UnityEngine;

namespace Tankito
{
    public class BulletCannon : NetworkBehaviour
    {
        public GameObject m_bulletPrefab;
        public BulletProperties m_bulletProperties;
        private List<BulletModifier> m_bulletModifiers;

        // Esto deberia ser comun para todos los bullets no deberia ser algo de cada disparador
        [SerializeField]
        float interval = 1;
        float timer = 0;
        List<GameObject> bulletsShot = new List<GameObject>();
        private List<Vector2> BulletDirection;
        public int baseBulletAmount;
        int bulletAmount;
        int spawnTickTime = 0;

        public List<BulletModifier> Modifiers { get => m_bulletModifiers; }
        public BulletProperties Properties { get => m_bulletProperties; }

        private void Start()
        {
            if (IsServer)
            {
                ApplyModifierProperties();
                SynchronizeBulletPropertiesClientRpc(m_bulletProperties);
            }
        }

        public void ApplyModifierProperties()
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
        
        public void Shoot()
        {
            // Probablemente sea razonable añadir un intervalo de buffer de inputs, de modo que si disparas justo antes de que se acabe el cooldown, dispare en cuanto se pueda. - Bernat
            if (IsServer)
            {
                SpawnBulletClientRpc();
            }

            // Spawn FX and do respective animations
        }

        [ClientRpc]
        void SpawnBulletClientRpc()
        {
            if (timer > interval)
            {
                // Bernat: No tengo ni idea de que es esto, lo voy a comentar
                
                //Vector2 direction;
                //float angle;
                //foreach (var item in BulletDirection)
                //{
                //    for (int i = 0; i < bulletAmount; i++)
                //    {
                //        if (bulletAmount % 2 == 0)
                //        {
//
                //        }
                //    }
                //}

                timer = 0;
                m_bulletProperties.direction = transform.right;
                m_bulletProperties.startingPosition = transform.position;
                m_bulletProperties.spawnTickTime = SimClock.TickCounter;
                var newBullet = NetworkObjectPool.Singleton.GetNetworkObject(bulletPrefab, transform.position, transform.rotation).gameObject;
                newBullet.transform.rotation= Quaternion.LookRotation(new Vector3(0, 0, 1), transform.right);
                newBullet.GetComponent<ABullet>().SetProperties(m_bulletProperties);
                newBullet.GetComponent<ABullet>().m_shooterID = shooterID;
                foreach (BulletModifier bulletModifier in m_bulletModifiers)
                {
                    bulletModifier.ConnectModifier(newBullet.GetComponent<ABullet>());
                }
                newBullet.GetComponent<BaseBullet>()?.Init();
                ShootClientRpc(newBullet.GetComponent<NetworkObject>().NetworkObjectId);
            }
        }
        [ClientRpc]
        void ShootClientRpc(ulong id)
        {
            ABullet bullet;
        }

        void Update()
        {
            if (IsServer)
            {
                timer += Time.deltaTime;
            }
            
        }
    }
}
