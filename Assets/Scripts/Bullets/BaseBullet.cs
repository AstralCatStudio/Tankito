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

        public override void Init()
        {
            base.Init();
            m_bouncesLeft = m_properties.bouncesTotal;
            //m_explosion = NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs[2].Prefab;
            m_rb.velocity = m_properties.velocity* m_properties.direction;
        }

        private void Update()
        {
            transform.rotation = Quaternion.LookRotation(new Vector3(0, 0, 1), m_rb.velocity.normalized);
            m_rb.velocity += (m_properties.acceleration != 0f) ? m_properties.acceleration * m_rb.velocity.normalized : Vector2.zero;
            m_lifetime += Time.deltaTime;
            if (IsServer)
            {
                if (m_lifetime >= m_properties.lifetimeTotal)
                {
                    Debug.Log($"lifetime: {m_lifetime}/{m_properties.lifetimeTotal}");
                    Detonate();
                }
            }
            
        }

        public void Detonate()
        {
            
            if (IsServer)
            {
                OnDetonate.Invoke(this);
                DetonateClientRpc();
                var networkObject = gameObject.GetComponent<NetworkObject>();
                networkObject.Despawn();
            }
            else
            {
                OnDetonate.Invoke(this);
                gameObject.SetActive(false);
            }
        }

        [ClientRpc]
        void DetonateClientRpc()
        {
                OnDetonate.Invoke(this);
        }
        
        private void OnCollisionEnter2D(Collision2D collision)
        {
            lastCollisionNormal = collision.GetContact(0).normal;
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
                    if(collision.gameObject.GetComponent<NetworkObject>().OwnerClientId == m_shooterID && m_lifetime < 0.03f)
                    {
                        //Debug.Log("Ignoing firing self collision");
                        //Detonate();
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
