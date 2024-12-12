using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tankito
{
    [System.Serializable]
    public struct HullStatsModifier
    {
        public float speedMultiplier;
        public float rotationSpeedMultiplier;
        public int extraHealth;
        public float extraParryTime;
        public float parryCooldownTimeAdded;
        public float dashSpeedMultiplier;
        public float dashDistanceMultiplier;
        public float dashCooldownTimeAdded;
        public bool invencibleDash;

        public HullStatsModifier(float speed, float rotSpeed, float dashCooldown, int health, float parryTime, float parryCooldown, float dashSpeed, float dashDistance, bool invencibleDash)
        {
            speedMultiplier = speed;
            rotationSpeedMultiplier = rotSpeed;
            dashCooldownTimeAdded = dashCooldown;
            extraHealth = health;
            extraParryTime = parryTime;
            parryCooldownTimeAdded = parryCooldown;
            dashSpeedMultiplier = dashSpeed;
            dashDistanceMultiplier = dashDistance;
            this.invencibleDash = invencibleDash;

        }
    }
    [CreateAssetMenu(menuName = "Modificadores/ModificadorTanque", order = 3, fileName = "Nuevo Modificador Tanque")]
    public class HullModifier : ScriptableObject
    {
        public HullStatsModifier hullStatsModifier;
        //TODO conectar los eventos del modificador al clientpredictedtankcontroller
    }
}