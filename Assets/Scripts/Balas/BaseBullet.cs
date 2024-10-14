using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Tankito
{
    public class BaseBullet : ABullet
    {
        [SerializeField]
        private Rigidbody2D rb;
        private float TTL = 5;
        private float lifetime = 0;
        private void Start()
        {
            rb.velocity = bulletProperties.velocity* bulletProperties.direction;
            transform.position = bulletProperties.startingPosition;
        }
        private void Update()
        {
            //transform.rotation = Quaternion.LookRotation(rb.velocity.normalized);
            //transform.rotation.SetLookRotation(rb.velocity.normalized);
            rb.velocity += bulletProperties.acceleration* bulletProperties.direction;
            lifetime += Time.deltaTime;

            if (lifetime >= TTL)
            {
                GetComponent<NetworkObject>().Despawn();
            }

        }
    }
}
