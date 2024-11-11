using System.Collections;
using System.Collections.Generic;
using Tankito;
using UnityEngine;
using Unity.Netcode;
using Tankito.Netcode;

namespace Tankito
{
    [System.Serializable]
    public class CreateExplosion : NetworkBehaviour
    {
        public GameObject explosionPrefab;
        public float size;
        public Vector3 relativePosition;
        public float totalLifetime;
        public float timeUntilBig;
        public void  StartEvent(ABullet bullet)
        {
            if (IsServer)
            {
                GameObject explosion = Instantiate<GameObject>(explosionPrefab, bullet.transform.position, bullet.transform.rotation);
                explosion.GetComponent<Explosion>().size *= size;
                explosion.transform.position += relativePosition;
                explosion.GetComponent<Explosion>().timeUntilBig=timeUntilBig;
                explosion.GetComponent<Explosion>().timeUntilDead = totalLifetime;
                explosion.GetComponent<NetworkObject>().Spawn();
            }
        }
    }
}
