using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tankito
{
    [CreateAssetMenu(menuName = "Modificadores/ListaModificadores", order = 4, fileName = "Nueva Lista Modificadores")]
    public class ModifierList : ScriptableObject
    {
        //pa acceder a la lista y solo tener que mandar por network el indice en vez de el modificador entero
        //en teoría solo habría uno de estos en todo el proyecto
        public List<Modifier> modifiers = new List<Modifier>();
    }
}
