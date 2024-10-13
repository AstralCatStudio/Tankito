using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tankito
{
    public class BaseBullet : ABullet
    {
        [SerializeField]
        private Rigidbody2D rb;
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
        }
    }
}
