using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tankito.SinglePlayer
{
    public class PVECharacterData : MonoBehaviour
    {
        [SerializeField] protected int m_maxHealth = 2;
        [SerializeField] protected int m_health;
        [SerializeField] protected bool m_isAlive;

        public int Max_Health => m_maxHealth;
        public int Health => m_health;
        public bool Alive => m_isAlive;

        protected virtual void Start()
        {
            m_health = m_maxHealth;
            m_isAlive = true;
        }

        public void TakeDamage(int damage)
        {
            m_health -= damage;

            if (m_health <= 0) Die();
        }

        public void AddHealth(int addedHealth)
        {
            m_health = (m_health + addedHealth > m_maxHealth) ? m_maxHealth : m_health + addedHealth;
        }

        protected virtual void Die()
        {
            Debug.Log($"El personaje {gameObject.name} fue derrotado");
            m_isAlive = false;
            gameObject.SetActive(false);

        }

        public void Revive()
        {
            Debug.Log($"El personaje {gameObject.name} fue derrotado");
            m_health = m_maxHealth;
            m_isAlive = true;
            gameObject.SetActive(true);
        }

        public void Revive(int newHealth)
        {
            Debug.Log($"El personaje {gameObject.name} fue derrotado");
            m_health = newHealth;
            m_isAlive = true;
            gameObject.SetActive(true);
        }
    }
}

