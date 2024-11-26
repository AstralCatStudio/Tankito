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
        private void Update()
        {
            timer += Time.deltaTime;
                if (timer >= timeUntilDead)
                {
                    //GetComponent<NetworkObject>().Despawn();
                    Destroy(gameObject);
                }
            
            
            if (timer <= timeUntilBig)
            {
                transform.localScale = Mathf.Lerp(0, 1, timer / timeUntilBig) * Vector3.one * size;
            }
            else
            {
                transform.localScale = Mathf.Lerp(0, 1, (timeUntilDead - timeUntilBig) - (timer- timeUntilBig) / (timeUntilDead- timeUntilBig)) * Vector3.one * size;
            }
            
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            
            if (collision.gameObject.CompareTag("Player"))
            {
                collision.GetComponent<TankData>().TakeDamage(1);
                Destroy(gameObject);
            }
        }
    }
}
