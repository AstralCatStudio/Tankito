using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
namespace Tankito
{
    
    [CreateAssetMenu(menuName = "Modificadores/Eventos/Explosion", order = 1, fileName = "Nuevo evento de explosion")]
    public class ExplosionEvent : AModifierEvent
    {
        CreateExplosion createExplosion;
        [SerializeField]
        GameObject explosionPrefab;
        [SerializeField]
        float size =1;
        [SerializeField]
        Vector2 relativePosition;
        [SerializeField]
        float totalLifetime = 1f;
        [SerializeField]
        float timeUntilBig = 0.5f;
        public override void StartEvent(ABullet bullet)
        {
            createExplosion.explosionPrefab = explosionPrefab;
            createExplosion.size = size;
            createExplosion.relativePosition = relativePosition;
            createExplosion.totalLifetime = totalLifetime;
            createExplosion.timeUntilBig = timeUntilBig;
            createExplosion.StartEvent(bullet);
        }
    }
}
