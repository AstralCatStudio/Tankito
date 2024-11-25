using Tankito.Netcode.Messaging;
using Unity.Netcode;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Tankito
{

    public class RoundManager : NetworkBehaviour
    {
        private int m_currentRound = 0;
        public int m_maxRounds = 5;
        [SerializeField]
        float timeToCountdown = 5f;
        private float m_currentCountdownTime;

        private Dictionary<ulong, TankData> m_players;
        public bool m_startedGame;
        public bool IsGameStarted => m_startedGame;
        private bool m_startedRound;

        public GameObject m_localPlayerInputObject;
        [SerializeField] private bool DEBUG = false;

        public delegate void RoundStart(int nRound);
        public event RoundStart OnPreRoundStart = (int nRound) => { };

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
            m_players = new Dictionary<ulong, TankData>();
            m_localPlayerInputObject = FindObjectOfType<PlayerInput>().gameObject;
        }

        void Start()
        {
            m_startedGame = false;
            m_startedRound = false;

            if (!m_startedGame)
            {
                m_localPlayerInputObject.SetActive(false);
            }

            //NetworkManager.Singleton.OnClientConnectedCallback += InitPlayersDictionary;
        }

        public void UpdateRemoteClientPlayerList()
        {
            if (!IsServer) return;
            //Debug.LogWarning($"Jugadores conectados: {m_players.Count}");
            //ulong[] clientIds = new ulong[m_players.Count];
            //int i = 0;
            //foreach (ulong id in m_players.Keys)
            //{
            //    clientIds[i] = id;
            //    Debug.LogWarning($"id anadido al array {clientIds[i]}");
            //    i++;
            //}
            //Debug.LogWarning($"i: {i}");
            //Debug.LogWarning($"Array complete: {clientIds.Length}");
            //NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(NetworkManager.Singleton.LocalClientId, out var playerObject);
            NetworkBehaviourReference[] tankDataRefs = m_players.Values.Select(td => new NetworkBehaviourReference(td)).ToArray();
            InitPlayersDictionaryClientRpc(tankDataRefs);
        }

        [ClientRpc]
        private void InitPlayersDictionaryClientRpc(NetworkBehaviourReference[] tankDataReferences)
        {
            foreach(var tankDataRef in tankDataReferences)
            {
                TankData tankData;
                tankDataRef.TryGet(out tankData);
                TryAddPlayer(tankData);
            }
            Debug.Log(m_players.Count);
            // Debug.LogWarning($"AAAA: {ids.Length}");
            // if (serverObject.TryGet(out var targetObject))
            // {
            //     if (!m_players.ContainsKey(ids[0]))
            //     {
            //         AddPlayer(targetObject.GetComponent<TankData>());
            //     }
            // }
            //     
            // for (int i = 0; i < ids.Length; i++)
            // {
            //     Debug.LogWarning($"EEEEE: {ids[i]}");
            //     if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(ids[i], out var playerObject))
            //     {
            //         Debug.LogWarning($"Anade {ids[i]}");
            //         if (!m_players.ContainsKey(ids[i]))
            //         {
            //             AddPlayer(playerObject.GetComponent<TankData>());
            //         }
            //     }
            // }
        }

        #region PlayerManagement
        public bool TryAddPlayer(TankData player)
        {
            if (m_players.ContainsKey(player.OwnerClientId))
            {
                return false;
            }
            AddPlayer(player);
            return true;
        }

        public void AddPlayer(TankData player)
        {
            m_players.Add(player.OwnerClientId, player);
            Debug.LogWarning($"Jugador anadido, {m_players.Count} jugadores");
            PlayerListUpdate();
        }

        public void RemovePlayer(ulong clientId)
        {
            m_players.Remove(clientId);
            PlayerListUpdate();
        }

        public void TankDeath(TankData t)
        {
            if (IsServer)
            {
                t.AwardPoints(m_players.Count - AliveTanks.Count()); // -1 porque no deberia darte puntos por estar tu mismo muerto
            }

            PlayerListUpdate(true);
            Debug.Log($"Round Manager registered a tank death: Tank[{t.OwnerClientId}]-Points: {t.Points}");
        }

        private void PlayerListUpdate(bool updateInputs = false)
        {
            UpdateAliveTanksGUI();
            if (updateInputs && m_startedGame)
            {
                foreach (var tank in m_players.Values)
                {
                    SetActiveTankInputs(tank);
                }
            }

            if (m_startedGame)
            {
                CheckForWinner();
            }
        }
        #endregion



        #region Countdown
        /// <summary>
        /// Only called by the server
        /// </summary>
        public void StartRoundCountdown()
        {
            RoundUI.Instance.SetActiveScenarySelection(false);
            StartRoundCountdown(m_currentRound);
        }

        /// <summary>
        /// Only called by the server
        /// </summary>
        public void StartRoundCountdown(int newRound)
        {
            ResetPlayers();
            // To avoid SimClocks diverging in the between rounds phase
            MessageHandlers.Instance.SendSynchronizationSignal();
            OnPreRoundStart?.Invoke(newRound);

            Debug.Log("Inicio ronda " + newRound);
            UpdateAliveTanksGUI();

            m_startedGame = true;
            m_currentCountdownTime = timeToCountdown;
            ActivateCountdownGUIClientRpc();
            CancelInvoke(nameof(UpdateCountdown));
            InvokeRepeating(nameof(UpdateCountdown), 0f, 1f);

            if (DEBUG) Debug.Log("Cuenta atras iniciada");

            m_localPlayerInputObject.SetActive(false);

            ClockSignal signal = new ClockSignal();
            signal.header = ClockSignalHeader.Start;
            MessageHandlers.Instance.SendClockSignal(signal);
        }

        /// <summary>
        /// Only called by the server
        /// </summary>
        private void UpdateCountdown()
        {
            if (m_currentCountdownTime > 0)
            {
                SetCountdownGUIClientRpc(m_currentCountdownTime.ToString());
                m_currentCountdownTime--;
            }
            else
            {
                CancelInvoke(nameof(UpdateCountdown));
                EndCountdown();
            }
        }

        /// <summary>
        /// Only called by the server
        /// </summary>
        private void EndCountdown()
        {
            if (DEBUG) Debug.Log("Fin de cuenta atras");

            SetCountdownGUIClientRpc("BATTLE!");
            Invoke(nameof(StartRound), 0.7f);
        }

        [ClientRpc]
        private void SetCountdownGUIClientRpc(string text)
        {
            RoundUI.Instance.SetCountdownText(text);
        }

        [ClientRpc]
        private void ActivateCountdownGUIClientRpc()
        {
            RoundUI.Instance.ActivateCountdownGUI(true);
        }

        [ClientRpc]
        private void DeactivateCountdownGUIClientRpc()
        {
            RoundUI.Instance.ActivateCountdownGUI(false);
        }
        #endregion



        #region Game State Helpers & GUIs
        /// <summary>
        /// Should only be called by the server
        /// </summary>
        private void ResetPlayers()
        {
            RespawnTanks();
            FindObjectOfType<SpawnManager>().ResetSpawnPoints();
        }

        private void RespawnTanks()
        {
            if (IsServer)
            {
                RespawnTanksClientRpc();
            }

            foreach (var tank in m_players.Values)
            {
                tank.ResetTank();
            }
            
            foreach (var item in m_players)
            {
                item.Value.gameObject.SetActive(true);
            }
        }

        [ClientRpc]
        private void RespawnTanksClientRpc()
        {
            if (!IsServer)
            {
                RespawnTanks();
            }
        }

        private void UpdateAliveTanksGUI()
        {
            RoundUI.Instance.SetRemainingPlayers(AliveTanks.Count());
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
        #endregion



        #region Round Logic


        public void StartRound()
        {            
            if (IsServer)
            {
                StartRoundClientRpc();
            }
            
            m_startedRound = true;
            RoundUI.Instance.ActivateLobbyInfoGUI(false);
            RoundUI.Instance.ActivateCountdownGUI(false);
            RoundUI.Instance.ActivateInitExitButton(false);
            RoundUI.Instance.ActivateAliveTanksGUI(true);
            m_localPlayerInputObject.SetActive(true);
            PlayerListUpdate();
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
                // To avoid SimClocks diverging in the between rounds phase
                MessageHandlers.Instance.SendSynchronizationSignal();
            }

            m_startedRound = false;
            m_currentRound++;
            if (DEBUG) Debug.Log("NETLESS: Fin de ronda");
            m_localPlayerInputObject.SetActive(false);
            RoundUI.Instance.ActivateAliveTanksGUI(false);
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

        private void CheckForWinner()
        {
            var nAlive = AliveTanks.Count();

            switch (nAlive)
            {
                case 1:
                    var winner = AliveTanks.First();
                    winner.AwardPoints(m_players.Count);
                    if (DEBUG) Debug.Log($"{winner} ha ganado la ronda");
                    if (IsServer) EndRound();
                    break;

                case 0:
                    if (DEBUG) Debug.Log($"Nadie ha ganado la ronda, EMPATE!");
                    if (IsServer) EndRound();
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
                Invoke(nameof(StartPowerUpSelection), 3.0f);

            }
            else
            {
                ShowRanking();
                Invoke(nameof(EndGame), 5.0f);
            }
        }
        #endregion



        #region RankingScreen
        private string GenerateRanking()
        {
            string rankingStr;
            if (m_currentRound == m_maxRounds)
            {
                rankingStr = "Ranking Final: ";
            }
            else
            {
                rankingStr = "Ranking: ";
            }

            TankData[] tanksByPoints = m_players.Values.OrderByDescending(tank => tank.Points).ToArray();

            for (int i = 0; i < tanksByPoints.Length; i++)
            {
                rankingStr += $"\n{i + 1}. Jugador {tanksByPoints[i].OwnerClientId}:  {tanksByPoints[i].Points} puntos";
            }

            return rankingStr;
        }

        public List<TankData> GetTankOrder()
        {
            return m_players.Values.OrderBy(tank => tank.Points).ToList<TankData>();
        }

        private void ShowRanking()
        {
            Debug.Log("Rankgin Screen show called.");

            if (IsServer)
            {
                ShowRankingClientRpc();
            }

            RoundUI.Instance.ActivateRankingGUI(true);
            RoundUI.Instance.SetRankingText(GenerateRanking());
        }

        [ClientRpc]
        private void ShowRankingClientRpc()
        {
            if (!IsServer)
            {
                ShowRanking();
            }
        }

        private void ShowFinalRanking()
        {
            if (IsServer)
            {
                ShowFinalRankingClientRpc();
            }

            if (DEBUG) Debug.Log("Se muestra el ranking final");
            RoundUI.Instance.SetActiveRankingFinal(true);
            RoundUI.Instance.SetRankingText(GenerateRanking());
        }

        [ClientRpc]
        private void ShowFinalRankingClientRpc()
        {
            if (!IsServer)
            {
                ShowFinalRanking();
            }
        }
        #endregion



        #region PowerupScreen
        private void StartPowerUpSelection()
        {
            RoundUI.Instance.SetActivePowerUps(true);
            RoundUI.Instance.ActivateRankingGUI(false);
        }

        public void EndPowerUpSelection()
        {
            if (IsServer)
            {
                EndPowerUpSelectionClientRpc();
            }

            RoundUI.Instance.SetActivePowerUps(false);
            if (IsServer)
            {
                Invoke(nameof(StartRoundCountdown), 1.0f);
            }
        }

        [ClientRpc]
        public void EndPowerUpSelectionClientRpc()
        {
            if (!IsServer)
            {
                EndPowerUpSelection();
            }
        }
        #endregion



        #region Game End
        private void EndGame()
        {
            if (IsServer)
            {
                EndGameClientRpc();
            }

            if (DEBUG) Debug.Log("Fin de la partida");
            m_startedGame = false;
            //RoundUI.Instance.ActivateRankingGUI(false);
            RoundUI.Instance.ActivateEndExitButton(true);
        }

        [ClientRpc]
        private void EndGameClientRpc()
        {
            if (!IsServer)
            {
                EndGame();
            }
        }
        #endregion



        #region DEBUG Methods
        [ContextMenu("TestDamageLocalPlayer")]
        public void TestDamagePlayer()
        {
            //m_players[NetworkManager.Singleton.LocalClientId].TakeDamage(1);
        }
        #endregion
    }
}