using System;
using System.Collections.Generic;
using System.ComponentModel;
using Tankito.Netcode;
using Tankito.Utils;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

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
        internal bool gameSceneLoaded = false;
        

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

            AutoPhysics2DUpdate(false);

            //_playerName = "Invited";
        }

        public void FindPlayerInput()
        {
            m_playerInput= GameObject.FindObjectOfType<PlayerInput>();
            m_inputActions = new TankitoInputActions();
            m_playerInput.actions = m_inputActions.asset;
        }

       public void BindInputActions()
        {
            // VA A FALLAR PARA CLIENTES (en principio funciona en hosts)
            
                var predictedController = NetworkManager.LocalClient.PlayerObject.GetComponent<ClientPredictedTankController>();
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
            // TODO: Unbind actions along with end of tank lifetime.

        }
        private void OnServerStarted()
        {
            print("Servidor inicalizado.");        
        }

        private void OnClientConnected(ulong clientId)
        {

            if (m_playerInput == null)
            {
                //FindPlayerInput();
            }

            NetworkObject newPlayer = null;
            if (IsServer && gameSceneLoaded)
            {
                print("Cliente se conecta");
                newPlayer = Instantiate(m_playerPrefab).GetComponent<NetworkObject>();

                newPlayer.SpawnAsPlayerObject(clientId);
            }
        }

        private void OnClientDisconnect(ulong obj)
        {
            throw new NotImplementedException();
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

        internal void CreatePlayer()
        {
            if (IsHost && gameSceneLoaded)
            {
                // IMPORTANTE: Siempre instanciar objetos con la sobrecarga de parentesco para asegurar la escena en la que residen
                // (evitando su destruccion no intencionada al cargarse sobre escenas aditivas que se descargan posteriormente eg. LA PANTALLA DE CARGA)
                var newPlayer = Instantiate(m_playerPrefab, GameInstanceParent.Instance.transform).GetComponent<NetworkObject>();

                newPlayer.SpawnAsPlayerObject(NetworkManager.LocalClientId);
            }
        }

        [ContextMenu("StartSimulationClocks")]
        internal void StartSimulationClocks()
        {
            if (IsServer)
            {
                ClockManager.StartClockClientRpc();
            }
        }
    }
}