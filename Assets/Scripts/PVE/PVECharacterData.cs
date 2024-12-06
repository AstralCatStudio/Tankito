using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tankito.SinglePlayer
{
    public enum tankType
    {
        player = 0,
        attacker = 1,
        bodyguard = 2,
        healer = 3,
        kamikaze = 4,
        miner = 5,
        necromancer = 6
    }

    public class PVECharacterData : MonoBehaviour
    {
        [SerializeField] protected int m_maxHealth = 2;
        [SerializeField] protected int m_health;
        [SerializeField] protected bool m_isAlive;

        [Header("Health Bar Settings")]
        [SerializeField] private GameObject healthBarPrefab; // Prefab del sprite de la barra de vida.
        [SerializeField] private Vector3 healthBarOffset = new Vector3(0, 1, 0); // Posición relativa de la barra de vida.
        [SerializeField] private Vector2 healthBarSize = new Vector2(1, 0.1f); // Tamaño inicial de la barra de vida.

        private Transform healthBarTransform; // Transform del sprite de la barra de vida.
        private SpriteRenderer healthBarRenderer; // Renderer para personalizar el color.

        public int Max_Health => m_maxHealth;
        public int Health => m_health;
        public bool Alive => m_isAlive;

        protected virtual void Start()
        {
            m_health = m_maxHealth;
            m_isAlive = true;

            // prefab de vida
            if (healthBarPrefab != null)
            {
                GameObject healthBarObject = Instantiate(healthBarPrefab, transform.position + healthBarOffset, Quaternion.identity, transform);
                healthBarTransform = healthBarObject.transform;
                healthBarRenderer = healthBarObject.GetComponent<SpriteRenderer>();

                // tamaño inicial del prfab
                healthBarTransform.localScale = new Vector3(healthBarSize.x, healthBarSize.y, 1);
                UpdateHealthBar();
            }
        }

        public void TakeDamage(int damage)
        {
            m_health -= damage;

            if (healthBarTransform != null)
            {
                UpdateHealthBar();
            }

            if (m_health <= 0) Die();
        }

        public void AddHealth(int addedHealth)
        {
            m_health = Mathf.Clamp(m_health + addedHealth, 0, m_maxHealth);

            if (healthBarTransform != null)
            {
                UpdateHealthBar();
            }
        }

        protected virtual void Die()
        {
            Debug.Log($"El personaje {gameObject.name} fue derrotado");
            m_isAlive = false;
            gameObject.SetActive(false);

            if (healthBarTransform != null)
            {
                healthBarTransform.gameObject.SetActive(false);
            }
        }

        public void Revive()
        {
            m_health = m_maxHealth;
            m_isAlive = true;
            gameObject.SetActive(true);

            if (healthBarTransform != null)
            {
                UpdateHealthBar();
                healthBarTransform.gameObject.SetActive(true);
            }
        }

        public void Revive(int newHealth)
        {
            m_health = Mathf.Clamp(newHealth, 0, m_maxHealth);
            m_isAlive = true;
            gameObject.SetActive(true);

            if (healthBarTransform != null)
            {
                UpdateHealthBar();
                healthBarTransform.gameObject.SetActive(true);
            }
        }

        private void UpdateHealthBar()
        {
            // porcentaje de vida restante
            float healthPercent = (float)m_health / m_maxHealth;

            // escalar
            healthBarTransform.localScale = new Vector3(healthPercent * healthBarSize.x, healthBarSize.y, 1);

            // cambiar color
            if (healthBarRenderer != null)
            {
                healthBarRenderer.color = Color.Lerp(Color.red, Color.green, healthPercent);
            }
        }
    }
}
