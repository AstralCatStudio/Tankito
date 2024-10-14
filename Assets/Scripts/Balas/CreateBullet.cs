using System.Collections;
using System.Collections.Generic;
using Tankito.Netcode;
using Unity.Netcode;
using UnityEngine;

namespace Tankito
{
    public class CreateBullet : MonoBehaviour
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
            if(timer> interval)
            {
                m_bulletProperties.direction = transform.up;
                m_bulletProperties.startingPosition = transform.position;
                timer = 0;
                GameObject newBullet;

                if (NetworkManager.Singleton.IsServer)
                {
                    newBullet = Instantiate(bulletPrefab, transform.position, transform.rotation);
                    newBullet.GetComponent<NetworkObject>().Spawn();
                    newBullet.GetComponent<ABullet>().SetProperties(m_bulletProperties);
                }
                
            }
        }
    }
}
