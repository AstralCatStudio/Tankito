using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tankito
{
    [System.Serializable]
    public struct HulltStatsModifier
    {
        public float speedMultiplier;
        public float rotationSpeedMultiplier;
        public float reloadTimeAdded;
        public float dashCooldownTimeAdded;
        public int extraHealth;
    }
    [CreateAssetMenu(menuName = "Modificadores/ModificadorTanque", order = 3, fileName = "Nuevo Modificador Tanque")]
    public class HullModifier : ScriptableObject
    {
        public HulltStatsModifier hullStatsModifier;
        //TODO conectar los eventos del modificador al clientpredictedtankcontroller
    }
}