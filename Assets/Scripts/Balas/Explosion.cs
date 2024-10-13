using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tankito
{
    public class Explosion : ABulletEvent
    {
        private Vector3 m_position;
        override public void StartEvent(ABullet bullet)
        {
            m_position = bullet.transform.position;
        }
    }
}
