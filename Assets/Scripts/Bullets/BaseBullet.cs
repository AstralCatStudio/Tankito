using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Tankito
{
    public class BaseBullet : ABullet
    {
        [SerializeField]
        private Rigidbody2D m_rb;
        private GameObject m_explosion;
        public void Start()
        {
            m_bouncesLeft = m_properties.bouncesTotal;
            m_explosion = NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs[2].Prefab;
            m_rb.velocity = m_properties.velocity* m_properties.direction;
            transform.position = m_properties.startingPosition;
        }
        private void Update()
        {
            //transform.rotation = Quaternion.LookRotation(rb.velocity.normalized);
            transform.rotation = Quaternion.LookRotation(new Vector3(0,0,1), m_rb.velocity.normalized);//Quaternion.LookRotation((Vector3)m_rb.velocity.normalized, Vector3.Cross((Vector3)m_rb.velocity.normalized, (Vector3)m_rb.velocity.normalized));
            m_rb.velocity += (m_properties.acceleration != 0f) ? m_properties.acceleration * m_properties.direction : Vector2.zero;
            m_lifetime += Time.deltaTime;

            if (m_properties.lifetimeTotal <= m_lifetime)
            {
                Detonate();
            }
        }
        public void Detonate()
        {
            OnDetonate.Invoke();
            if (NetworkManager.Singleton.IsServer)
            {
                Instantiate(m_explosion,transform.position, transform.rotation);
            }
                // Return to the pool from whence it came.
                var networkObject = gameObject.GetComponent<NetworkObject>();
            networkObject.Despawn();
        }
        private void OnCollisionEnter2D(Collision2D collision)
        {
            switch (collision.gameObject.tag)
            {
                case "NormalWall":
                    if (m_bouncesLeft <= 0)
                    {
                        Detonate();
                    }
                    else
                    {
                        m_bouncesLeft--;
                    }
                    break;
                case "BouncyWall":

                    break;

                case "Player":
                    if(collision.gameObject.GetComponent<NetworkObject>().OwnerClientId == m_ownerID)
                    {

                    }
                    else
                    {
                        Detonate();
                    }
                    break;

                default:
                    Detonate();
                    break;
            }
        }
    }
}
