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
        [SerializeField]
        public BulletProperties m_bulletProperties;
        [SerializeField]
        float interval = 1;
        float timer = 0;
        private

        void Start()
        {
            
        }
        public void Shoot()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                if (timer > interval)
                {
                    timer = 0;
                    m_bulletProperties.direction = transform.right;
                    m_bulletProperties.startingPosition = transform.position;
                    var newBullet = NetworkObjectPool.Singleton.GetNetworkObject(bulletPrefab, transform.position, transform.rotation).gameObject;
                    newBullet.GetComponent<ABullet>().SetProperties(m_bulletProperties);
                    //newBullet.GetComponent<ABullet>().m_ownerID = gameObject.GetComponent<NetworkObject>().OwnerClientId;
                    newBullet.GetComponent<NetworkObject>().Spawn();
                    newBullet.GetComponent<BaseBullet>()?.Start();
                }
            }
        }
        void Update()
        {
            timer += Time.deltaTime;
        }
    }
}
