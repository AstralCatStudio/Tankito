using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using Tankito;
using NUnit.Framework;
using Tankito.Netcode.Simulation;
using UnityEngine.Rendering;

namespace Tankito
{
    public class TankData : NetworkBehaviour
    {
        public delegate void TankDestroyedHandler(TankData tank);
        public event TankDestroyedHandler OnTankDestroyed = (TankData tank) => { };
        //public Action<TankData> OnDamaged = (TankData damagedTank) => { };
        private int m_health;
        private bool m_isAlive;
        private int m_points;
        public int Health => m_health;
        public bool Alive => m_isAlive;
        public int Points => m_points;

        void Start()
        {
            if (IsServer)
            {
                m_points = 0;
            }
        }

        public void Die()
        {
            if (IsServer)
            {
                DieClientRpc();
            }

            OnTankDestroyed.Invoke(this);
            UnityEngine.Debug.LogWarning("TODO: Trigger tank death animation");
            gameObject.SetActive(false);
        }

        [ClientRpc]
        public void DieClientRpc()
        {
            if (!IsServer)
            {
                Die();
            }
        }
        private void OnEnable()
        {
            OnTankDestroyed += RoundManager.Instance.TankDeath;
        }
        private void OnDisable()
        {

            OnTankDestroyed -= RoundManager.Instance.TankDeath;
        }
        /// <summary>
        /// Should Only Be Called  from the server OR as a clientRpc (e.g from the server)
        /// </summary>
        /// <param name="awardedPoints"></param>
        public void AwardPoints(int awardedPoints)
        {
            if (IsServer)
            {
                AwardPointsClientRpc(awardedPoints);
            }
            m_points += awardedPoints;

        }
        [ClientRpc]
        void AwardPointsClientRpc(int awardedPoints)
        {
            if (!IsServer)
            {
                AwardPoints(awardedPoints);
            }

        }
        private void Update()
        {
            //Debug.Log("Vida actual: "+ m_health);
        }
        public void TakeDamage(int damage)
        {
            //OnDamaged(this);
            if (IsServer)
            {
                m_health -= damage;

                if (m_health <= 0)
                {
                    m_isAlive = false;
                    Die();
                }
            }
        }

        public void AddHealth(int addedHealth)
        {
            if (IsServer)
            {
                m_health += addedHealth;
            }

        }
        public void SetHealth(int newHealth)
        {
            if (IsServer)
            {
                m_health = newHealth;
            }
        }

        public void ResetTank()
        {
            //Debug.LogWarning("TODO: maybe play spawn animation?");
            gameObject.SetActive(true);
            if (IsServer)
            {
                m_isAlive = true;
            }
        }
    }
}
