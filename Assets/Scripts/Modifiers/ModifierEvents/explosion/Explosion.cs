using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;

namespace Tankito
{
    public class Explosion : MonoBehaviour
    {
        public float size = 1;
        public float timer = 0;
        public float timeUntilBig = 0.5f;
        public float timeUntilDead = 1;
        public bool damages = false;
        private void Start()
        {
            transform.localScale = Vector3.one*size;
        }
        private void Update()
        {
            timer += Time.deltaTime;
                if (timer >= timeUntilBig)
                {
                    //GetComponent<NetworkObject>().Despawn();
                    GetComponent<CircleCollider2D>().enabled = false;
                }
            if (timer >= timeUntilDead)
            {
                Destroy(gameObject);
            }

        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (damages && collision.gameObject.CompareTag("Player"))
            {
                collision.GetComponent<TankData>().TakeDamage(1);
            }
        }
    }
}
