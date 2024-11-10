using System.Collections;
using System.Collections.Generic;
using Tankito;
using UnityEngine;
using Unity.Netcode;
using Tankito.Netcode;

namespace Tankito
{
    [System.Serializable]
    public class CreateExplosion : ABulletEvent
    {

        [SerializeField]
        private GameObject m_explosion;
        public override void  StartEvent(ABullet bullet)
        {
            if (IsServer)
            {
                GameObject explosion = Instantiate<GameObject>(m_explosion);
                explosion.GetComponent<NetworkObject>().Spawn();
            }
        }
    }
}
