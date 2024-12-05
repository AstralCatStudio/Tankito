using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tankito.SinglePlayer
{
    [CreateAssetMenu(menuName = "Npc/NpcData", order = 2, fileName = "New NpcData")]
    public class NpcData : ScriptableObject
    {
        public int health;
        public float speed;
        public float aimSpeed;
        public float angularSpeed;
        public float idealDistance;
        public float attackRange;
        public float reloadTime;
        public float angleErrorAccepted; //Margen de error del angulo para disparar
        public string genericTargetTag;
    }
}

