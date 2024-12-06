using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tankito.SinglePlayer
{
    public class PVEEnemyData : PVECharacterData
    {
        public delegate void EnemyDeathEvent();
        public event EnemyDeathEvent OnDeath;

        protected override void Start()
        {
            m_maxHealth = GetComponent<AgentController>().npcData.health;
            base.Start();
        }

        protected override void Die()
        {
            OnDeath?.Invoke();
            base.Die();
            Debug.Log("Se crea resto para revivir con el necromancer");
            Instantiate(GetComponent<AgentController>().npcData.leftoversInDeath, transform.position, Quaternion.identity);
        }
    }
}

