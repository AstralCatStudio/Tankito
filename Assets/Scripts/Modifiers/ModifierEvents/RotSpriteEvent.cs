using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tankito
{
    [CreateAssetMenu(menuName = "Modificadores/Eventos/RotSprite", order = 1, fileName = "Nuevo evento de rotar sprite")]
    public class RotSpriteEvent : ABulletModifierEvent
    {
        [SerializeField] float rotSpeed;
        public override void StartEvent(BulletController bullet)
        {
            if (SimClock.Instance.Active == false) return;

            if(bullet.Velocity.sqrMagnitude > 0.05f)
            {
                bullet.transform.GetChild(0).transform.Rotate(new Vector3(0, 0, rotSpeed * bullet.Velocity.sqrMagnitude * SimClock.SimDeltaTime));
            }
        }
    }
}

