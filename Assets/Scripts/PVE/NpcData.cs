using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tankito.SinglePlayer
{
    [CreateAssetMenu(menuName = "Npc/NpcData", order = 2, fileName = "New NpcData")]
    public class NpcData : ScriptableObject
    {
        public float speed;
        public float angularSpeed;
        public float idealDistance;
        public float runAwayDistance;
        public float attackRange;
        public float reloadTime;
        public string genericTargetTag;
    }
}

