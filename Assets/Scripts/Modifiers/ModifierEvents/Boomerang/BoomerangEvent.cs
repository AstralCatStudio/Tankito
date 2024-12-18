using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tankito
{
    [CreateAssetMenu(menuName = "Modificadores/Eventos/Boomerang", order = 1, fileName = "Nuevo evento de Boomerang")]
    public class BoomerangEvent : ABulletModifierEvent
    {
        public override void StartEvent(BulletController bullet)
        {
            bullet.BulletType = BulletMoveType.Boomerang;
        }
    }
}

