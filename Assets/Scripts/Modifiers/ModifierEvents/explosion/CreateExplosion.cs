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
                Debug.Log("Hola");

                GameObject explosion = Instantiate<GameObject>(explosionPrefab, bullet.transform.position, bullet.transform.rotation);
                
                explosion.GetComponent<Explosion>().size *= size;
                explosion.transform.position += relativePosition;
                explosion.GetComponent<Explosion>().timeUntilBig = timeUntilBig;
                explosion.GetComponent<Explosion>().timeUntilDead = totalLifetime;
                explosion.GetComponent<NetworkObject>().SpawnWithOwnership(bullet.GetComponent<NetworkObject>().OwnerClientId);
                syncronizeExplosionClientRpc(explosion.GetComponent<NetworkObject>().NetworkObjectId);
                Destroy(this);
            }
            

        }
        [ClientRpc]
        void syncronizeExplosionClientRpc(ulong explosionID)
        {
            Debug.Log("Hola");
            NetworkObject explosion=null;
            foreach (var item in FindObjectsOfType<NetworkObject>())
            {
                if(item.NetworkObjectId== explosionID)
                {
                    explosion = item;
                }
            }
            if(explosion != null)
            {
                explosion.GetComponent<Explosion>().size *= size;
                explosion.transform.position += relativePosition;
                explosion.GetComponent<Explosion>().timeUntilBig = timeUntilBig;
                explosion.GetComponent<Explosion>().timeUntilDead = totalLifetime;
            }
        }
    }
}
