using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tankito
{
    [System.Serializable]
    public struct HulltStatsModifier
    {
        public float speedMultiplier;
        public float reloadTimeAdded;
        public float dashCooldownTimeAdded;
        public int extraHealth;
        public float amountAdded;
        public float amountMultiplier;
        public float spreadMultiplier;
    }
    [CreateAssetMenu(menuName = "Modificadores/ModificadorTanque", order = 3, fileName = "Nuevo Modificador Tanque")]
    public class HullModifier : ScriptableObject
    {
        public HulltStatsModifier hullStatsModifier;
    }
}