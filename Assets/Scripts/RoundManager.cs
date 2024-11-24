using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;
using Tankito.Netcode.Messaging;
using System;

namespace Tankito
{
    public class RoundManager : NetworkBehaviour
    {
        private int m_currentRound = 0;
        public int m_maxRounds = 5;

        private string m_ranking;

        public RoundUI m_roundUI;

        const float timeToCountdown = 5f;
        private float m_currentCountdownTime;

        private Dictionary<ulong, TankData> m_players = new Dictionary<ulong, TankData>();
        public bool m_startedGame;
        public bool IsGameStarted => m_startedGame;
        private bool m_startedRound;

        private SpawnManager m_spawnManager;
        public GameObject m_localPlayerInputObject;
        [SerializeField] private bool DEBUG = false;

        public delegate void RoundStart(int nRound);
        public event RoundStart OnRoundStart;

        public static RoundManager Instance { get; private set; }
        public IEnumerable<TankData> AliveTanks { get => m_players.Where(p => p.Value.Alive == true).Select(p => p.Value); }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }
        }

        void Start()
        {
            m_startedGame = false;
            m_startedRound = false;

            m_localPlayerInputObject = GameObject.Find("PlayerInput");

            if (IsServer)
            {
                m_spawnManager = GameObject.Find("SpawnManager").GetComponent<SpawnManager>();
            }

            if (!m_startedGame)
            {
                m_localPlayerInputObject.SetActive(false);
            }

            m_roundUI = FindObjectOfType<RoundUI>();
        }

        #region PlayerManagement
        public void AddPlayer(TankData player)
        {
            m_players.Add(player.OwnerClientId, player);
            PlayerListUpdate();
        }

        public void RemovePlayer(ulong clientId)
        {
            m_players.Remove(clientId);
            PlayerListUpdate();
        }


        private void OnEnable()
        {
            if (DEBUG) Debug.Log("Se suscribe al evento de morir tanque");
            TankData.OnTankDestroyed += TankDeath;
        }

        private void OnDisable()
        {
            if (DEBUG) Debug.Log("Se desuscribe al evento de morir tanque");
            TankData.OnTankDestroyed -= TankDeath;
        }

        private void TankDeath(TankData t)
        {
            t.AwardPoints(m_players.Count - AliveTanks.Count() - 1); // -1 porque no deberia darte puntos por estar tu mismo muerto
            PlayerListUpdate();
        }

        private void PlayerListUpdate()
        {
            UpdateAliveTanksGUI();
            foreach(var tank in m_players.Values)
            {
                SetActiveTankInputs(tank);
            }
            CheckForWinner();
        }
        #endregion


        [ContextMenu("TestDamageLocalPlayer")]
        public void TestDamagePlayer()
        {
            m_players[NetworkManager.Singleton.LocalClientId].TakeDamage(1);
        }

        public void StartRoundCountdown()
        {
            StartRoundCountdown(m_currentRound++);
        }

        public void StartRoundCountdown(int newRound)
        {
            m_roundUI.SetActivePowerUps(false);
            //DisablePowerUpsClientRpc();

            ResetPlayers();

            Debug.Log("Inicio ronda " + newRound);
            UpdateAliveTanksGUI();
            StartCountdown();

            ClockSignal signal = new ClockSignal();
            signal.header = ClockSignalHeader.Start;
            MessageHandlers.Instance.SendClockSignal(signal);

            OnRoundStart?.Invoke(newRound);
        }

        private void ResetPlayers()
        {
            m_spawnManager.ResetSpawnPoints();

            foreach (var tank in AliveTanks)
            {
                tank.ResetTank();
            }
        }

        private void UpdateAliveTanksGUI()
        {
            m_roundUI.SetRemainingPlayers(AliveTanks.Count());
        }

        private void SetActiveTankInputs(TankData tank)
        {
            var active = tank.Alive;

            // tank.GetComponent<ITankInput>().SetActive(active);

            if (tank.IsLocalPlayer)
            {
                m_localPlayerInputObject.SetActive(active);
            }
        }

        #region Countdown

        private void StartCountdown()
        {
            m_startedGame = true;
            m_currentCountdownTime = timeToCountdown;
            StartCountdownClientRpc();
            CancelInvoke(nameof(UpdateCountdown));
            InvokeRepeating(nameof(UpdateCountdown), 0f, 1f);

            if (DEBUG) Debug.Log("Cuenta atras iniciada");

            m_localPlayerInputObject.SetActive(false);
        }

        private void UpdateCountdown()
        {
            if (m_currentCountdownTime > 0)
            {
                SetCountdownTextClientRpc(m_currentCountdownTime.ToString());
                m_currentCountdownTime--;
            }
            else
            {
                CancelInvoke(nameof(UpdateCountdown));
                EndCountdown();
            }
        }

        private void EndCountdown()
        {
            if (DEBUG) Debug.Log("Fin de cuenta atras");

            SetCountdownTextClientRpc("BATTLE!");
            Invoke(nameof(StartRound), 0.7f);
        }

        [ClientRpc]
        private void SetCountdownTextClientRpc(string text)
        {
            m_roundUI.SetCountdownText(text);
        }

        [ClientRpc]
        private void StartCountdownClientRpc()
        {
            m_roundUI.SetActiveCountownText(true);
        }

        [ClientRpc]
        private void EndCountdownClientRpc()
        {
            m_roundUI.SetActiveCountownText(false);
        }

        #endregion
    /*
        #region PlayerInputManagement
        public void DisablePlayerInput()
        {
            m_playerInputObject.SetActive(false);
        }

        [ClientRpc]
        private void DisablePlayerInputClientRpc()
        {
            if (m_playerInputObject != null)
            {
                Debug.Log("Player input desactivado");
                m_playerInputObject.SetActive(false);
            }
            else
            {
                Debug.Log("Player input no encontrado");
            }
        }

        [ClientRpc]
        private void DisablePlayerInputClientRpc(ulong clientId)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                //Debug.Log("Desactivo input porque me han derrotado");
                if (m_playerInputObject != null)
                {
                    Debug.Log("Player input desactivado");
                    m_playerInputObject.SetActive(false);
                }
                else
                {
                    Debug.Log("Player input no encontrado");
                }
            }
        }

        [ClientRpc]
        private void EnablePlayerInputClientRpc()
        {
            if (m_playerInputObject != null)
            {
                m_playerInputObject.SetActive(true);
            }
        }

        #endregion
    */
        #region FlujoPartida
        public void StartRound()
        {
            if (IsServer)
            {
                StartRoundClientRpc();
            }
            
            m_startedRound = true;
            m_roundUI.ActivateAliveTanksGUI(true);
            m_localPlayerInputObject.SetActive(true);
        }

        [ClientRpc]
        public void StartRoundClientRpc()
        {
            if (!IsServer)
            {
                StartRound();
            }
        }

        private void EndRound()
        {
            if (IsServer)
            {
                EndRoundClientRpc();
            }
            
            m_startedRound = false;
            if (DEBUG) Debug.Log("NETLESS: Fin de ronda");
            m_localPlayerInputObject.SetActive(false);
            m_roundUI.ActivateAliveTanksGUI(false);
            BetweenRounds();
        }

        [ClientRpc]
        private void EndRoundClientRpc()
        {
            if (!IsServer)
            {
                EndRound();
            }
        }

        [ClientRpc]
        //private void DisablePowerUpsClientRpc()
        {
            m_roundUI.SetActivePowerUps(false);
        }

        private void CheckForWinner()
        {
            var nAlive = AliveTanks.Count();

            switch(nAlive)
            {
                case 1:
                    var winner = AliveTanks.First();
                    if (DEBUG) Debug.Log($"{winner} ha ganado la ronda");
                    EndRound();
                    break;

                case 0:
                    if (DEBUG) Debug.Log($"Nadie ha ganado la ronda, EMPATE!");
                    EndRound();
                    break;

                default:
                    if (DEBUG) Debug.Log("La ronda sigue");
                    break;
            }
        }

        private void BetweenRounds()
        {
            if (m_currentRound < m_maxRounds)
            {
                ShowRanking();
                Invoke(nameof(PowerUpSelection), 3.0f);
            }
            else
            {
                ShowRanking();
                Invoke(nameof(EndGame), 5.0f);
            }
        }

        private void ShowRanking()
        {
            if (DEBUG) Debug.Log("NETLESS: Se muestra el ranking");
            GenerateRanking();
            ShowRankingClientRpc(m_ranking);
        }

        [ClientRpc]
        //private void ShowRankingClientRpc(string ranking)
        {
            if (DEBUG) Debug.Log("NETCODE: Se muestra el ranking en todos");
            m_roundUI.SetActiveRanking(true);
            m_roundUI.SetRankingText(ranking);
        }

        private void ShowFinalRanking()
        {
            if (DEBUG) Debug.Log("NETLESS: Se muestra el ranking final");
            //_roundUI.SetActiveRankingFinal(true);
            ShowFinalRankingClientRpc();
        }

        [ClientRpc]
        //private void ShowFinalRankingClientRpc()
        {
            if (DEBUG) Debug.Log("NETCODE: Se muestra el ranking final en todos");
            m_roundUI.SetActiveRankingFinal(true);
        }

        private void PowerUpSelection()
        {
            if (DEBUG) Debug.Log("NETLESS: Se eligen power ups");
            m_roundUI.SetActiveRanking(false);
            m_roundUI.SetActivePowerUps(true);
            ShowPowerUpsClientRpc();
        }

        public void EndPowerUpSelection()
        {
            if (IsServer)
            {
                Invoke(nameof(StartRoundCountdown), 1.0f);
            }
        }

        [ClientRpc]
        //private void ShowPowerUpsClientRpc()
        {
            if (DEBUG) Debug.Log("NETCODE: Se muestran los power ups en todos");
            m_roundUI.SetActiveRanking(false);
            m_roundUI.SetActivePowerUps(true);
        }

        private void EndGame()
        {
            if (DEBUG) Debug.Log("NETLESS: Fin de la partida");
            m_startedGame = false;
            EndGameClientRpc();
        }

        [ClientRpc]
        private void EndGameClientRpc()
        {
            if (DEBUG) Debug.Log("NETCODE: Final de partida en todos");
        }
        #endregion

        private void GenerateRanking()
        {
            if (m_currentRound == m_maxRounds)
            {
                m_ranking = "Ranking Final: ";
            }
            else
            {
                m_ranking = "Ranking: ";
            }

            TankData[] tanksByPoints = m_players.Values.OrderByDescending(tank => tank.Points).ToArray();

            for (int i = 0; i < tanksByPoints.Length; i++)
            {
                m_ranking += $"\n{i + 1}. Jugador {tanksByPoints[i].OwnerClientId}:  {tanksByPoints[i].Points} puntos";
            }
        }
    }
}