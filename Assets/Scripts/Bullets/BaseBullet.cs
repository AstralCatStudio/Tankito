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
        private void Start()
        {
            m_rb.velocity = m_properties.velocity* m_properties.direction;
            transform.position = m_properties.startingPosition;
        }
        private void Update()
        {
            //transform.rotation = Quaternion.LookRotation(rb.velocity.normalized);
            //transform.rotation.SetLookRotation(rb.velocity.normalized);
            m_rb.velocity += (m_properties.acceleration != 0f) ? m_properties.acceleration * m_properties.direction : Vector2.zero;
            m_lifetime += Time.deltaTime;

            if (m_properties.lifetimeTotal <= m_lifetime)
            {
                // Return to the pool from whence it came.
                var networkObject = gameObject.GetComponent<NetworkObject>();
                networkObject.Despawn();
            }

        }
    }
}
