using System.Collections;
using System.Collections.Generic;
using Tankito;
using UnityEngine;
using Unity.Netcode;
using Tankito.Netcode;
using UnityEngine.SceneManagement;

namespace Tankito
{
    [System.Serializable]
    public class CreateExplosion : MonoBehaviour
    {
        public GameObject explosionPrefab;
        public float size;
        public Vector3 relativePosition;
        public float totalLifetime;
        public float timeUntilBig;
        public void  StartEvent(ABullet bullet)
        {
                GameObject explosion = Instantiate<GameObject>(explosionPrefab, bullet.transform.position, bullet.transform.rotation);
                explosion.GetComponent<Explosion>().size *= size;
                explosion.transform.position += relativePosition;
                explosion.GetComponent<Explosion>().timeUntilBig = timeUntilBig;
                explosion.GetComponent<Explosion>().timeUntilDead = totalLifetime;
        }
    }
}
