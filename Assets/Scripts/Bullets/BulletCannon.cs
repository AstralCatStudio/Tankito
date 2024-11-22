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
                    ShootBullet(aimVector);
                }
        }

        /*void SpawnSimulatedBullet(Vector2 aimVector)
        {
            m_bulletProperties.direction = aimVector;
            m_bulletProperties.startingPosition = transform.position;
            m_bulletProperties.spawnTickTime = SimClock.TickCounter;
            var newBullet = NetworkObjectPool.Singleton.GetNetworkObject(BulletCannonRegistry.Instance.m_bulletPrefab, transform.position, transform.rotation).gameObject;

            newBullet.SetActive(true);
            newBullet.GetComponent<BulletController>().SimulatedNetworkSpawn(OwnerClientId);
            Debug.Log("encolada la bala " + newBullet.GetComponent<NetworkObject>().NetworkObjectId);
            simulatedBullets.Enqueue(newBullet);
        }*/

        void ShootBullet(Vector2 aimVector)
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
            m_bulletProperties.direction = aimVector;
            m_bulletProperties.startingPosition = transform.position;
            m_bulletProperties.spawnTickTime = SimClock.TickCounter;
            //SpawnBulletClientRpc(m_bulletProperties.direction, m_bulletProperties.startingPosition, m_bulletProperties.spawnTickTime);

            
            var newBullet = BulletPool.Instance.Get(BulletCannonRegistry.Instance.m_bulletPrefab, transform.position, transform.rotation).gameObject;

            newBullet.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId);
        }

        /*[ClientRpc]
        void SpawnBulletClientRpc(Vector2 direction, Vector2 position, int tickCounter)
        {
            if(IsOwner && !IsServer)
            {
                GameObject bala = simulatedBullets.Dequeue();
                Debug.Log("desencolada la bala " + bala.GetComponent<NetworkObject>().NetworkObjectId);
                bala.GetComponent<SpriteRenderer>().color = Color.red;
                bala.SetActive(false);
                bala.GetComponent<BulletController>().OnNetworkDespawn();
                
            }
            m_bulletProperties.direction = direction;
            m_bulletProperties.startingPosition = position;
            m_bulletProperties.spawnTickTime = tickCounter;
        }*/

        void Update()
        {            
                timer += Time.deltaTime;
        }
    }
}
