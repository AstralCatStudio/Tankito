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

        [SerializeField]
        public Color playerColor;
        //public Action<TankData> OnDamaged = (TankData damagedTank) => { };
        private int m_health;
        private bool m_isAlive;
        private int m_points;
        private string m_username;
        private int m_skinSelected;
        public int Health => m_health;
        public bool Alive => m_isAlive;
        public int Points => m_points;
        public string Username => m_username;
        public int SkinSelected => m_skinSelected;

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
            MusicManager.Instance.PlaySound("snd_muere");
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
            if(IsOwner)
            {
                m_username = ClientData.Instance.username;
                m_skinSelected = ClientData.Instance.characters.IndexOf(ClientData.Instance.GetCharacterSelected());
                SetClientDataServerRpc(m_username, m_skinSelected);
            }
        }
        [ServerRpc]
        void SetClientDataServerRpc(string username, int skinSelected)
        {
            if (!IsOwner)
            {
                m_username = username;
                m_skinSelected = skinSelected;
                SetClientDataClientRpc(username, skinSelected);
            }
        }
        [ClientRpc]
        void SetClientDataClientRpc(string username, int skinSelected)
        {
            if (!IsOwner && !IsServer)
            {
                m_username = username;
                m_skinSelected = skinSelected;
            }
        }
        private void OnDisable()
        {

            OnTankDestroyed -= RoundManager.Instance.TankDeath;
        }
        
        private void Update()
        {
            //Debug.Log("Vida actual: "+ m_health);
        }
        public void TakeDamage(int damage)
        {
            //OnDamaged(this);

            if (SimClock.Instance.Active && NetworkManager.Singleton.IsClient)
                MusicManager.Instance.PlayDamage();

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

        #region Points
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

        public void ResetPoints()
        {
            if (IsServer)
            {
                ResetPointsClientRpc();
            }
            m_points = 0;
        }

        [ClientRpc]
        void ResetPointsClientRpc()
        {
            if (!IsServer)
            {
                ResetPoints();
            }
        }
        #endregion
    }
}
