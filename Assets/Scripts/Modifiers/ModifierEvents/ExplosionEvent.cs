using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Tankito
{
    [CreateAssetMenu(menuName = "Modificadores/Eventos/Explosion", order = 5, fileName = "Nuevo evento")]
    public class ExplosionEvent : ScriptableObject
    {
        public CreateExplosion createExplosion;
    }
}
