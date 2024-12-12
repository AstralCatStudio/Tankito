using Tankito.Netcode.Messaging;
using Unity.Netcode;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Tankito.ScenarySelection;
using Tankito.Mobile;
using Tankito.Netcode.Simulation;

namespace Tankito
{
    public class RoundManager : NetworkBehaviour
    {
        [SerializeField] private int m_currentRound = 0;
        public int m_maxRounds = 5;
        [SerializeField]
        float timeToCountdown = 5f;
        private float m_currentCountdownTime;

        private Dictionary<ulong, TankData> m_players;
        public bool m_startedGame;
        public bool IsGameStarted => m_startedGame;
        [SerializeField] private bool m_startedRound;
        public bool IsRoundStarted => m_startedRound;

        public GameObject m_localPlayerInputObject;
        [SerializeField] private bool DEBUG = false;

        public delegate void RoundStart(int nRound);
        public event RoundStart OnPreRoundStart = (int nRound) => { };

        public static RoundManager Instance { get; private set; }
        public IEnumerable<TankData> AliveTanks { get => m_players.Where(p => p.Value.Alive == true).Select(p => p.Value); }
        public bool IsAlive(ulong clientId) => m_players[clientId].Alive;
        public List<TankData> playerList;
        public Dictionary<ulong, TankData> Players { get => m_players; }
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
            NetworkBehaviourReference[] tankDataRefs = m_players.Values.Select(td => new NetworkBehaviourReference(td)).ToArray();
            InitPlayersDictionaryClientRpc(tankDataRefs);
        }

        [ClientRpc]
        private void InitPlayersDictionaryClientRpc(NetworkBehaviourReference[] tankDataReferences)
        {
            foreach (var tankDataRef in tankDataReferences)
            {
                TankData tankData;
                tankDataRef.TryGet(out tankData);
                TryAddPlayer(tankData);
            }
            Debug.Log(m_players.Count);
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
            playerList.Add(player);
            m_players.Add(player.OwnerClientId, player);
            foreach (TankData playerdata in m_players.Values)
            {
                playerdata.GetComponent<TankSkinController>().SetOwnedSkin();
            }
            Debug.LogWarning($"Jugador anadido, {m_players.Count} jugadores");
            PlayerListUpdate();
        }

        public void RemovePlayer(ulong clientId)
        {
            playerList.Remove(m_players[clientId]);
            m_players.Remove(clientId);
            PlayerListUpdate();
        }

        public void TankDeath(TankData t)
        {
            if (!m_startedRound) return;

            if (IsServer)
            {
                t.AwardPoints(m_players.Count - AliveTanks.Count());
            }

            if (AliveTanks.Count() != 1) // Cuando muere un bicho y no es el ultimo
            {
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                FasePartidaClientRpc(AliveTanks.Count(), m_players.Count); // primero jugadores vivos, despues jugadores totales
            }

            PlayerListUpdate(true);
            Debug.Log($"Round Manager registered a tank death: Tank[{t.OwnerClientId}]-Points: {t.Points}");
        }

        private void PlayerListUpdate(bool updateInputs = false)
        {
            UpdateRoundCounterGUI();
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


        #region ClientDataProvisional

        [ClientRpc]
        private void InitRankingClientRpc()
        {
            RoundUI.Instance.InitRanking(playerList);
        }

        //private void SetColors()
        //{
        //    for(int i = 0; i < m_players.Count; i++)
        //    {
        //        playerList[i].SetClientDataColor(i);
        //    }
        //}

        #endregion

        #region Countdown
        /// <summary>
        /// Only called by the server
        /// </summary>
        public void StartRoundCountdown()
        {
            if (m_currentRound == 0)
            {
                MusicManager.Instance.MuteSong();
            }

            RoundUI.Instance.SetActiveScenarySelection(false);

            InitRankingClientRpc();
            
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
            UpdateRoundCounterGUI();

            DeactivateInitExitButtonClientRpc();

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

                SemaforoSoundClientRpc(0); ////////////////////////////////////////////////////////////////////////////////////////

                m_currentCountdownTime--;
            }
            else
            {
                CancelInvoke(nameof(UpdateCountdown));

                SemaforoSoundClientRpc(1); ////////////////////////////////////////////////////////////////////////////////////////

                EndCountdown();
            }
        }

        /// <summary>
        /// Only called by the server
        /// </summary>
        private void EndCountdown()
        {
            if (DEBUG) Debug.Log("Fin de cuenta atras");

            if (m_currentRound == 0)
            {
                InitPartidaMusicClientRpc(ScenarySelector.Instance.GetActiveBiome()); ////////////////////////////////////////////////////////////////////////////////////////
            }
            FasePartidaClientRpc(AliveTanks.Count(), m_players.Count); ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Debug.Log($"Music manager: {AliveTanks.Count()}, {m_players.Count}");

            SetCountdownGUIClientRpc("BATTLE!");
            Invoke(nameof(StartRound), 0.7f);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [ClientRpc]
        private void SemaforoSoundClientRpc(int sound)
        {
            if (m_currentRound == 0)
            {
                MusicManager.Instance.MuteSong();
            }

            if (sound == 0)
            {
                MusicManager.Instance.Semaforo0();
            }
            else
            {
                MusicManager.Instance.Semaforo1();
            }
        }

        [ClientRpc]
        private void InitPartidaMusicClientRpc(int biome)
        {
            MusicManager.Instance.InitPartida(biome); // 0 - playa, 1 - sushi, 2 - barco
        }

        [ClientRpc]
        private void FasePartidaClientRpc(int vivos, int totales)
        {
            MusicManager.Instance.FasePartida(vivos, totales); // primero jugadores vivos, despues jugadores totales
            //Debug.Log($"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA Music manager AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA: {vivos}, {totales}");
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

        [ClientRpc]
        private void DeactivateInitExitButtonClientRpc()
        {
            RoundUI.Instance.ActivateInitExitButton(false);
        }
        #endregion



        #region Game State Helpers & GUIs
        /// <summary>
        /// Should only be called by the server
        /// </summary>
        private void ResetPlayers()
        {
            RespawnTanks();
            //ScenarySelector scenarySelector = FindObjectOfType<ScenarySelector>();
            if (ScenarySelector.Instance != null)
            {
                ScenarySelector.Instance.SetRandomMap();
            }
            else
            {
                Debug.LogWarning("Selector de escenario no encontrado");
            }
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

        // Bernat: He hecho un hack super feo, esto antes era el numero de jugadores vivos, pero como no tenia mucho sentido lo he cambiaado y ya.
        // pero no he cambiado los callsites de la funcion, porque con tal de que se actualice me da igual xd
        private void UpdateRoundCounterGUI()
        {
            RoundUI.Instance.SetCurrentRound(m_currentRound + 1);
        }

        private void SetActiveTankInputs(TankData tank)
        {
            var active = tank.Alive;

            // tank.GetComponent<ITankInput>().SetActive(active);

            if (tank.IsLocalPlayer && !RoundUI.Instance.SettingsMenu.activeSelf)
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

            FindObjectOfType<TouchControlManager>().ReleaseForceHideTouchGUI();
            m_startedRound = true;
            RoundUI.Instance.ActivateLobbyInfoGUI(false);
            RoundUI.Instance.ActivateCountdownGUI(false);
            //RoundUI.Instance.ActivateInitExitButton(false);
            RoundUI.Instance.ActivateAliveTanksGUI(true);

            if (!RoundUI.Instance.SettingsMenu.activeSelf) m_localPlayerInputObject.SetActive(true);

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

                ClockSignal signal = new ClockSignal();
                signal.header = ClockSignalHeader.Stop;
                MessageHandlers.Instance.SendClockSignal(signal);

                //SimClock.Instance.StopClock();
                //ServerSimulationManager.Instance.ClearBullets();
                ClearSimObjClientRpc();
            }

            m_startedRound = false;
            m_currentRound++;

            if (RoundUI.Instance.SettingsMenu.activeSelf)
            {
                RoundUI.Instance.ActivateSettingsMenu(false);
            }

            if (RoundUI.Instance.SettingsButton.activeSelf)
            {
                RoundUI.Instance.ActivateSettingsButton(false);
            }

            if (m_currentRound == m_maxRounds)
            {
                MusicManager.Instance.FinPartida();
            }


            if (DEBUG) Debug.Log("NETLESS: Fin de ronda");
            m_localPlayerInputObject.SetActive(false);
            RoundUI.Instance.ActivateAliveTanksGUI(false);
            BetweenRounds();
        }

        [ClientRpc]
        private void ClearSimObjClientRpc()
        {
            if(ClientSimulationManager.Instance != null)
            {
                ClientSimulationManager.Instance.ClearBullets();
            }
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

        // Client + Server
        private void BetweenRounds()
        {
            FindObjectOfType<TouchControlManager>().ForceHideTouchGUI();

            if (m_currentRound < m_maxRounds && m_players.Count > 1)
            {
                ShowRanking();

                MusicManager.Instance.FaseEntrerrondas();

                Invoke(nameof(StartPowerUpSelection), 3.0f);

            }
            else
            {
                ShowRanking();
                Invoke(nameof(EndGame), 3.0f);
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
            //RoundUI.Instance.SetRankingText(GenerateRanking());

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            if (m_currentRound == m_maxRounds)
            {
                List<TankData> lista = GetTankOrder();
                Debug.Log($"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA: Resultados: {lista[lista.Count - 1].GetComponent<NetworkObject>().OwnerClientId == NetworkManager.Singleton.LocalClientId}");
                MusicManager.Instance.Resultados(lista[lista.Count - 1].GetComponent<NetworkObject>().OwnerClientId == NetworkManager.Singleton.LocalClientId);
                //Debug.Log($"Resultados {lista[lista.Count - 1].GetComponent<NetworkObject>().OwnerClientId == NetworkManager.Singleton.LocalClientId}");
            }

        }

        [ClientRpc]
        private void ShowRankingClientRpc()
        {
            if (!IsServer)
            {
                ShowRanking();
            }
        }

        //private void ShowFinalRanking()
        //{
        //    if (IsServer)
        //    {
        //        ShowFinalRankingClientRpc();
        //    }

        //    if (DEBUG) Debug.Log("Se muestra el ranking final");
        //    RoundUI.Instance.SetActiveRankingFinal(true);
        //    RoundUI.Instance.SetRankingText(GenerateRanking());
        //}

        //[ClientRpc]
        //private void ShowFinalRankingClientRpc()
        //{
        //    if (!IsServer)
        //    {
        //        ShowFinalRanking();
        //    }
        //}



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
            RoundUI.Instance.ActivateSettingsButton(true);
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
                //EndGameClientRpc();

                /*ClockSignal signal = new ClockSignal();
                signal.header = ClockSignalHeader.Stop;
                MessageHandlers.Instance.SendClockSignal(signal);*/
            }

            if (DEBUG) Debug.Log("Fin de la partida");
            m_startedGame = false;
            RoundUI.Instance.ActivateRankingGUI(false);

            RoundUI.Instance.InitWinScreen(m_players.Values.OrderByDescending(tank => tank.Points).ToList());
            RoundUI.Instance.ActivateWinScreen(true);

            RoundUI.Instance.ActivateEndExitButton(true);
            RoundUI.Instance.ActivatePlayAgainGUI(true);
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



        #region GameReset

        public void ResetGame()
        {
            if (IsServer)
            {
                RoundUI.Instance.SetActiveScenarySelection(true);
                ResetGameClientRpc();

                foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
                {
                    ResetTanksModifiersClientRpc(clientId);
                }

                foreach (var tank in m_players)
                {
                    tank.Value.ResetPoints();
                }

                ResetPlayers();
            }
        }

        [ClientRpc]
        private void ResetGameClientRpc()
        {
            m_currentRound = 0;
            m_startedGame = false;
            m_startedRound = false;
            RoundUI.Instance.SetCurrentRound(m_currentRound);
            RoundUI.Instance.ActivateEndExitButton(false);
            RoundUI.Instance.ActivateRankingGUI(false);
            RoundUI.Instance.ActivatePlayAgainGUI(false);
            RoundUI.Instance.ActivateWinScreen(false);
            RoundUI.Instance.ResetWinScreen();

            RoundUI.Instance.ActivateInitExitButton(true);
            RoundUI.Instance.ActivateSettingsButton(true);
            RoundUI.Instance.ActivateLobbyInfoGUI(true);
            RoundUI.Instance.ActivateReadyGUI(true);
            RoundUI.Instance.SetCountdownText("Ready?");
            RoundUI.Instance.ActivateCountdownGUI(true);
        }

        [ClientRpc]
        private void ResetTanksModifiersClientRpc(ulong clientId)
        {
            BulletCannonRegistry.Instance[clientId].transform.parent.parent.parent.GetComponent<ModifiersController>().ResetModifiers();
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