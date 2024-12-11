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
            if(!bullet.IsStiCked) 
            bullet.transform.GetChild(0).transform.localRotation = Quaternion.Euler(0, 0, SimClock.TickCounter * rotSpeed * SimClock.SimDeltaTime);
        }
    }
}

