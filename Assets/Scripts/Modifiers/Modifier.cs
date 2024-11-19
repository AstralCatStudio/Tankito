using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tankito
{
    [CreateAssetMenu(menuName = "Modificadores/ModificadorBase", order = 1, fileName = "Nuevo Modificador")]
    public class Modifier : ScriptableObject
    {
        //contiene una lista de hull modifiers y bullet modifiers, este scriptable object será el que le llegue al tanque
        public List<BulletModifier> bulletModifiers = new List<BulletModifier>();
        public List<HullModifier> hullModifiers = new List<HullModifier>();
    }
}
