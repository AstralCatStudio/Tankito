using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tankito.SinglePlayer
{
    public class PVEEnemyData : PVECharacterData
    {
        protected override void Start()
        {
            m_maxHealth = GetComponent<AgentController>().npcData.health;
            base.Start();
        }

        protected override void Die()
        {
            base.Die();
            Debug.Log("Se crea resto para revivir con el necromancer");
            
        }
    }
}

