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
        BulletProperties m_bulletProperties;
        [SerializeField]
        float interval = 1;
        float timer = 0;
        private

        void Start()
        {
            
        }

        void Update()
        {
            timer += Time.deltaTime;
            if(timer > interval)
            {
                m_bulletProperties.direction = transform.up;
                m_bulletProperties.startingPosition = transform.position;
                timer = 0;

                if (NetworkManager.Singleton.IsServer)
                {
                    var newBullet = NetworkObjectPool.Singleton.GetNetworkObject(bulletPrefab, transform.position, transform.rotation).gameObject;
                    newBullet.GetComponent<ABullet>().SetProperties(m_bulletProperties);
                    newBullet.GetComponent<NetworkObject>().Spawn();
                }
                
            }
        }
    }
}
