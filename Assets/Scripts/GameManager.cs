using System;
using System.Collections.Generic;
using System.ComponentModel;
using Tankito.Netcode;
using Tankito.Utils;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Tankito
{
    public class GameManager : NetworkBehaviour
    {
        private NetworkManager m_networkManager;
        [SerializeField]
        private PlayerInput m_playerInput;
        private TankitoInputActions m_inputActions;

        //[SerializeField]
        private GameObject m_playerPrefab;

        private string m_playerName = "Invited";

        // SceneManagementEvents
        public bool m_loadingSceneFlag { get; private set; } = true;

        public string joinCode;

        public static GameManager Instance { get; private set; }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            m_networkManager = NetworkManager.Singleton;

            if (m_playerPrefab == null) m_playerPrefab = m_networkManager.NetworkConfig.Prefabs.Prefabs[0].Prefab;
            if (m_playerPrefab == null) Debug.LogWarning("Something went wron, couldn't obtain player prefab (frist prefab in networkconfig)");

            m_networkManager.OnServerStarted += OnServerStarted;
            m_networkManager.OnClientConnectedCallback += OnClientConnected;
            m_networkManager.OnClientDisconnectCallback += OnClientDisconnect;

            AutoPhysics2DUpdate(true);

        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                NetworkManager.SceneManager.OnLoadEventCompleted += (string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut) => OnSceneLoaded();
            }
            else
            {
                NetworkManager.SceneManager.OnSynchronizeComplete += (ulong syncedClientId) => OnSceneLoaded();
            }
        }

        private void OnServerStarted()
        {
            print("Servidor inicalizado.");        
        }

        private void OnClientConnected(ulong clientId)
        {
            Debug.Log("GameManager CLIENT CONNECTED called.");
            
            if (m_loadingSceneFlag)
            {
                Debug.Log("loadingScene");
                if (IsServer)
                { NetworkManager.SceneManager.OnLoadEventCompleted += (string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut) => OnClientConnected(clientId); }
                else
                { NetworkManager.SceneManager.OnSynchronizeComplete += (ulong syncedClientId) => OnClientConnected(clientId); }
                
                return;
            }
            else
            {
                Debug.Log("notLoading");
                if (IsServer)
                { NetworkManager.SceneManager.OnLoadEventCompleted -= (string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut) => OnClientConnected(clientId); }
                else
                { NetworkManager.SceneManager.OnSynchronizeComplete -= (ulong syncedClientId) => OnClientConnected(clientId); }
            }

            if (IsServer)
            {
                // IMPORTANTE: Siempre instanciar objetos con la sobrecarga de parentesco para asegurar la escena en la que residen
                // (evitando su destruccion no intencionada al cargarse sobre escenas aditivas que se descargan posteriormente eg. LA PANTALLA DE CARGA)
                var newPlayer = Instantiate(m_playerPrefab, GameInstanceParent.Instance.transform).GetComponent<NetworkObject>();

                newPlayer.SpawnAsPlayerObject(clientId);
            }
            FindPlayerInput();
            // BindInputActions(); Bound by the player controller itself on network spawn.
        }

        private void OnClientDisconnect(ulong obj)
        {
            throw new NotImplementedException();
        }
        
        private void OnSceneLoaded()
        {
            m_loadingSceneFlag = false;
        }
        
        public void UnloadScene()
        {
            // Assure only the server calls this when the NetworkObject is
            // spawned and the scene is loaded.
            if (!IsServer || !IsSpawned )//|| !sceneLoaded.IsValid() || !sceneLoaded.isLoaded) // ADAPTAR??
            {
                return;
            }

            m_loadingSceneFlag = true;
        }

        public void FindPlayerInput()
        {
            if (m_playerInput == null)
                m_playerInput = GameObject.FindObjectOfType<PlayerInput>();

            if (m_inputActions == null)
                m_inputActions = new TankitoInputActions();

            m_playerInput.actions = m_inputActions.asset;
        }

        public void BindInputActions(ClientPredictedTankController predictedController)
        {
            FindPlayerInput();

            Debug.Log($"{predictedController}");
            m_inputActions.Player.Move.performed += predictedController.OnMove;
            m_inputActions.Player.Move.canceled += predictedController.OnMove;
            m_inputActions.Player.Look.performed += predictedController.OnAim;
            m_inputActions.Player.Look.canceled += predictedController.OnAim;
            m_inputActions.Player.Dash.performed += predictedController.OnDash;
            //m_inputActions.Player.Dash.canceled += predictedController.OnDash;
            m_inputActions.Player.Parry.performed += predictedController.OnParry;
            m_inputActions.Player.Parry.canceled += predictedController.OnParry;
            m_inputActions.Player.Fire.performed += predictedController.OnFire;
            m_inputActions.Player.Fire.canceled += predictedController.OnFire;

        }


        public void SetPlayerName(string name)
        {
            m_playerName = name;
            Debug.Log("GameManager guarda el nombre:" + m_playerName);
        }

        public string GetPlayerName()
        {
            return m_playerName;
        }

        public void AutoPhysics2DUpdate(bool auto)
        {
            if (!auto)
            {
                Physics2D.simulationMode = SimulationMode2D.Script;
            }
            else
            {
                Physics2D.simulationMode = SimulationMode2D.FixedUpdate;
            }
        }

        [ContextMenu("StartSimulationClocks")]
        internal void StartSimulationClocks()
        {
            if (IsServer)
            {
                StartClocksClientRpc();
            }
            else
            {
                Debug.LogWarning("Simulation clocks must be started from server");
            }
        }

        [ClientRpc]
        public void StartClocksClientRpc()
        {
            Debug.Log("Starting client clocks...");
            ClockManager.Instance.StartClock();
            AutoPhysics2DUpdate(false);
        }
    }
}