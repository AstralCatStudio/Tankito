using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Tankito.Netcode;
using System;
namespace Tankito
{
    public class Explosion : MonoBehaviour
    {
        public float size = 1;
        public float timer = 0;
        public float timeUntilBig = 0.5f;
        public float timeUntilDead = 1;
        private void Update()
        {
            timer += Time.deltaTime;
            if (NetworkManager.Singleton.IsServer)
            {
                if (timer >= timeUntilDead)
                {
                    //GetComponent<NetworkObject>().Despawn();
                    Destroy(gameObject);
                }
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
    }
}
