using System;
using System.Collections;
using System.Collections.Generic;
using Tankito.Netcode;
using Unity.Netcode;
using UnityEngine;

namespace Tankito
{
    public class CreateBullet : NetworkBehaviour
    {
        public GameObject bulletPrefab;
        public BulletProperties m_bulletProperties;
        [SerializeField]
        public BulletProperties m_baseBulletProperties;
        [SerializeField]
        float interval = 1;
        float timer = 0;
        List<GameObject> bulletsShot = new List<GameObject>();
        public List<BulletModifier> modifiers;
        private void Start()
        {
            if (IsServer)
            {
                m_bulletProperties = m_baseBulletProperties;
                applyModifierProperties();
                SynchronizeBulletPropertiesClientRpc(m_bulletProperties);
            }
        }
        public void applyModifierProperties()
        {
            foreach (BulletModifier modifier in modifiers)
            {

                m_bulletProperties.velocity *= modifier.bulletStatsModifier.speedMultiplier;
                m_bulletProperties.scaleMultiplier *= modifier.bulletStatsModifier.sizeMultiplier;
                m_bulletProperties.acceleration += modifier.bulletStatsModifier.accelerationAdded;
                m_bulletProperties.bouncesTotal += modifier.bulletStatsModifier.BouncesAdded;
                m_bulletProperties.lifetimeTotal += modifier.bulletStatsModifier.lifeTimeAdded;
            }
        }
        public void Shoot()
        {
            // Probablemente sea razonable añadir un intervalo de buffer de inputs, de modo que si disparas justo antes de que se acabe el cooldown, dispare en cuanto se pueda. - Bernat
            if (IsOwner)
            {
                ShootServerRpc(OwnerClientId);
            }
        }

        [ServerRpc]
        void ShootServerRpc(ulong shooterID)
        {
            if (timer > interval)
            {
                timer = 0;
                m_bulletProperties.direction = transform.right;
                m_bulletProperties.startingPosition = transform.position;
                var newBullet = NetworkObjectPool.Singleton.GetNetworkObject(bulletPrefab, transform.position, transform.rotation).gameObject;
                newBullet.transform.rotation= Quaternion.LookRotation(new Vector3(0, 0, 1), transform.right);
                newBullet.GetComponent<ABullet>().SetProperties(m_bulletProperties);
                newBullet.GetComponent<ABullet>().m_shooterID = shooterID;
                foreach (BulletModifier bulletModifier in modifiers)
                {
                    bulletModifier.ConnectModifier(newBullet.GetComponent<ABullet>());
                }
                newBullet.GetComponent<NetworkObject>().Spawn();
                newBullet.GetComponent<BaseBullet>()?.Init();
            }
        }

        [ClientRpc]
        private void SynchronizeBulletPropertiesClientRpc(BulletProperties properties)
        {
            m_bulletProperties = properties;
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
