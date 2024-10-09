using System.Collections.Generic;
using Tankito.Netcode;
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

        [SerializeField]
        private GameObject m_playerPrefab;

        private string m_playerName = "Invited";

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

            AutoPhysics2DUpdate(false);

            m_inputActions = new TankitoInputActions();
            m_playerInput.actions = m_inputActions.asset;

            //_playerName = "Invited";
        }

        private void OnServerStarted()
        {
            print("Servidor inicalizado.");        
        }

        private void OnClientConnected(ulong clientId)
        {
            if (IsServer)
            {
                print("Cliente se conecta");
                var newPlayer = Instantiate(m_playerPrefab).GetComponent<NetworkObject>();

                newPlayer.SpawnAsPlayerObject(clientId);
                if (newPlayer.IsOwner)
                {
                    var predictedController = newPlayer.GetComponent<ClientPredictedTankController>();
                    Debug.Log($"{predictedController}");
                    m_inputActions.Player.Move.performed += predictedController.OnMove;
                    m_inputActions.Player.Move.canceled += predictedController.OnMove;
                    m_inputActions.Player.Look.performed += predictedController.OnAim;
                    m_inputActions.Player.Look.canceled += predictedController.OnAim;

                    // TODO: Unbind actions along with end of tank lifetime.
                }
            }
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
    }
}