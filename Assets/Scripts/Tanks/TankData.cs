using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using Tankito;
using NUnit.Framework;
using Tankito.Netcode.Simulation;
using UnityEngine.Rendering;
using System.Linq;

namespace Tankito
{
    public class TankData : NetworkBehaviour, IComparable
    {
        public delegate void TankDestroyedHandler(TankData tank);
        public event TankDestroyedHandler OnTankDestroyed = (TankData tank) => { };
        public int playerNumber;
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
        public int position;
        public GameObject playerInfo;
        private Color[] colors = { Color.blue, Color.red, Color.green, Color.yellow };
        public float damageBufferTime = 0.5f;
        public float damageBuffer = 0;
        public GameObject tankExplosion;
        public GameObject tankDamagedExplosion;
        public int CompareTo(object obj)
        {
            var a = this;
            var b = obj as TankData;

            if (a.m_points < b.m_points)
                return 1;
            else if (a.m_points > b.m_points)
                return -1;

            return 0;
        }
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
            Instantiate(tankExplosion, transform.position, transform.rotation);
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

        }

        private void OnDisable()
        {

            OnTankDestroyed -= RoundManager.Instance.TankDeath;
        }

        private void Update()
        {
            //Debug.Log("Vida actual: "+ m_health);
            if (IsServer)
            {
                damageBuffer += Time.deltaTime;
            }

        }
        public void TakeDamage(int damage)
        {
            //OnDamaged(this);
            Instantiate(tankDamagedExplosion, transform.position, transform.rotation);
            if (SimClock.Instance.Active && NetworkManager.Singleton.IsClient)
                MusicManager.Instance.PlayDamage();

            if (IsServer && damageBuffer >= damageBufferTime)
            {
                damageBuffer = 0;
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

        #region username
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsOwner)
            {
                m_username = ClientData.Instance.username;
                m_skinSelected = ClientData.Instance.characters.IndexOf(ClientData.Instance.GetCharacterSelected());
                //Debug.Log("Soy el jugador " + RoundManager.Instance.playerList.IndexOf(this));
                if (RoundManager.Instance.playerList.IndexOf(this) < 4 && RoundManager.Instance.playerList.IndexOf(this) >= 0)
                {
                    playerColor = colors[RoundManager.Instance.playerList.IndexOf(this)];

                }
                else
                {
                    Debug.Log("Color default");
                    playerColor = new Color(1, 1, 1, 1);
                }

                SetClientDataServerRpc(m_username, m_skinSelected);
            }
            if (IsServer)
            {
                AssignColor();

            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            if (IsServer)
            {
                AssignColor();
            }
        }

        private void AssignColor()
        {
            var clients = NetworkManager.Singleton.ConnectedClientsList;

            for (int i = 0; i < clients.Count; i++)
            {
                var client = clients[i];
                var playerObject = client.PlayerObject;

                if (playerObject != null && playerObject.TryGetComponent(out TankData tank))
                {
                    Color assignedColor = (i < colors.Length) ? colors[i] : Color.white;
                    tank.SetClientDataColor(assignedColor);
                }
            }
        }

        [ServerRpc]
        public void SetClientDataServerRpc(string username, int skinSelected)
        {
            m_username = username;
            m_skinSelected = skinSelected;
            SetClientDataClientRpc(username, skinSelected);

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

        public void SetClientDataColor(Color color)
        {
            if (IsServer)
            {
                SetClientDataColorClientRpc(color);
            }
        }
        
        [ClientRpc]
        private void SetClientDataColorClientRpc(Color color)
        {
            
                playerColor = color;
            
        }

        #endregion
    }
}
