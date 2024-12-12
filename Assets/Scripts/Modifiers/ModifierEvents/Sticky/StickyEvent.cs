using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tankito
{
    [CreateAssetMenu(menuName = "Modificadores/Eventos/Sticky", order = 1, fileName = "Nuevo evento de sticky")]
    public class StickyEvent : ABulletModifierEvent
    {
        public override void StartEvent(BulletController bullet)
        {
            if (SimClock.Instance.Active == false) return;
            bullet.BulletType = BulletMoveType.Sticky;
        }
    }
}