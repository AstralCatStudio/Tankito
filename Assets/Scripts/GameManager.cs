using System;
using System.Collections.Generic;
using System.ComponentModel;
using Tankito.Netcode;
using Tankito.Netcode.Simulation;
using Tankito.Utils;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Tankito.Netcode.Messaging;
using Tankito.ScenarySelection;

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
            if (m_playerPrefab == null) Debug.LogWarning("Something went wrong, couldn't obtain player prefab (frist prefab in networkconfig)");

            m_networkManager.OnServerStarted += OnServerStarted;
            m_networkManager.OnClientConnectedCallback += OnClientConnected;
            m_networkManager.OnClientDisconnectCallback += OnClientDisconnect;
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
            //print("Servidor inicalizado.");        
        }

        private void OnClientConnected(ulong clientId)
        {
            if (IsServer)
            {
                ServerSendSimulationParameters(clientId);
            }
            //Debug.Log("GameManager CLIENT CONNECTED called.");

            if (m_loadingSceneFlag)
            {
                //Debug.Log("loadingScene");
                if (IsServer)
                { NetworkManager.SceneManager.OnLoadEventCompleted += (string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut) => OnClientConnected(clientId); }
                else
                { NetworkManager.SceneManager.OnSynchronizeComplete += (ulong syncedClientId) => OnClientConnected(clientId); }

                return;
            }
            else
            {
                //Debug.Log("notLoading");
                if (IsServer)
                { NetworkManager.SceneManager.OnLoadEventCompleted -= (string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut) => OnClientConnected(clientId); }
                else
                { NetworkManager.SceneManager.OnSynchronizeComplete -= (ulong syncedClientId) => OnClientConnected(clientId); }
            }

            if (IsServer)
            {
                // Desconecta al jugador si ya ha empezado la partida
                if (RoundManager.Instance.IsGameStarted)
                {
                    Debug.LogWarning($"Rejecting client {clientId} - Game already in progress");
                    DisconnectHandler.Instance.DisconnectClientRpc(clientId);
                    return;
                }

                // Desconecta al jugador si ya hay 4 jugadores
                int connectedClients = NetworkManager.Singleton.ConnectedClientsIds.Count;
                if (connectedClients > 4)
                {
                    Debug.LogWarning($"Rejecting client {clientId} - Server full (4 players maximum)");
                    DisconnectHandler.Instance.DisconnectClientRpc(clientId);
                    return;
                }
            }


            if (IsServer)
            {
                // IMPORTANTE: Siempre instanciar objetos con la sobrecarga de parentesco para asegurar la escena en la que residen
                // (evitando su destruccion no intencionada al cargarse sobre escenas aditivas que se descargan posteriormente eg. LA PANTALLA DE CARGA)
                var newPlayer = Instantiate(m_playerPrefab, GameInstanceParent.Instance.transform).GetComponent<NetworkObject>();
                newPlayer.SpawnAsPlayerObject(clientId);

                // Primeras llamadas a round manager y spawn manager
                SpawnManager spawnManager = FindObjectOfType<SpawnManager>();
                spawnManager.SetPlayerInSpawn(clientId);

                var tankData = newPlayer.GetComponent<TankData>();
                if (tankData == null)
                {
                    Debug.LogException(new InvalidOperationException("Tank Data component not found!"));
                }
                Debug.Log($"TankData = {tankData}");
                RoundManager.Instance.AddPlayer(tankData);
                RoundManager.Instance.UpdateRemoteClientPlayerList();
            }

            if (clientId != NetworkManager.Singleton.LocalClientId) return;

            Debug.Log("Registering in Message Handler");

            NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(MessageName.ClockSignal, MessageHandlers.Instance.ReceiveClockSignal);
            NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(MessageName.InputWindow, MessageHandlers.Instance.ReceiveInputWindow);
            NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(MessageName.RelayInputWindow, MessageHandlers.Instance.ReceiveRelayedInputWindow);
            //NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(MessageName.AcnowledgeInputs, MessageHandlers.Instance.ReceiveInputAcknowledgement);
            NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(MessageName.SimulationSnapshot, MessageHandlers.Instance.ReceiveSimulationSnapshot);
        }

        private void ServerSendSimulationParameters(ulong clientId)
        {
            var medianLatency = (float)SimulationParameters.Instance.MedianLatency;
            var worstLatency = (float)SimulationParameters.Instance.WorstCaseLatency;
            var simTickRate = SimulationParameters.Instance.SimTickRate;

            ClientRpcParams target = new ClientRpcParams();
            target.Send.TargetClientIds = new ulong[] {clientId};
            SetSimulationParametersClientRpc(medianLatency, worstLatency, simTickRate, target);
        }

        [ClientRpc()]
        private void SetSimulationParametersClientRpc(float medianLatency, float worstCaseLatency, int simTickRate, ClientRpcParams clientRpcSendParams)
        {
            //Debug.Log("Received Simulation Parameters!");
            SimulationParameters.Instance.SetParams(medianLatency, worstCaseLatency, simTickRate);
        }

        private void OnClientDisconnect(ulong clientId)
        {
            //Debug.LogException(new NotImplementedException());

            //Gestionar desconexion del spawn manager
            if (IsServer)
            {
                SpawnManager spawnManager = FindObjectOfType<SpawnManager>();
                spawnManager.FreeSpawnPoint(clientId);

                RoundManager.Instance.RemovePlayer(clientId);
                RoundManager.Instance.UpdateRemoteClientPlayerList();

            }
        }

        private void OnSceneLoaded()
        {
            m_loadingSceneFlag = false;
        }

        public void OnSceneLoading()
        {
            m_loadingSceneFlag = true;
        }

        public void FindPlayerInput()
        {
            m_playerInput = GameObject.FindObjectOfType<PlayerInput>(true);
            if (m_playerInput == null)
            {
                Debug.LogWarning("Player input nulo");
            }
            else
            {
                //Debug.Log("Player input encontrado");
            }
            m_inputActions = new TankitoInputActions();
            m_playerInput.actions = m_inputActions.asset;
        }

        public void BindInputActions(TankPlayerInput localTankInput)
        {
            FindPlayerInput();

            m_inputActions.Player.Move.performed += localTankInput.OnMove;
            m_inputActions.Player.Move.canceled += localTankInput.OnMove;
            m_inputActions.Player.Look.performed += localTankInput.OnAim;
            m_inputActions.Player.Look.canceled += localTankInput.OnAim;
            m_inputActions.Player.Dash.performed += localTankInput.OnDash;
            //m_inputActions.Player.Dash.canceled += predictedController.OnDash;
            m_inputActions.Player.Parry.performed += localTankInput.OnParry;
            m_inputActions.Player.Parry.canceled += localTankInput.OnParry;
            m_inputActions.Player.Fire.performed += localTankInput.OnFire;
            m_inputActions.Player.Fire.canceled += localTankInput.OnFire;
        }

        public void UnbindInputActions(TankPlayerInput localTankInput)
        {
            m_inputActions.Player.Move.performed -= localTankInput.OnMove;
            m_inputActions.Player.Move.canceled -= localTankInput.OnMove;
            m_inputActions.Player.Look.performed -= localTankInput.OnAim;
            m_inputActions.Player.Look.canceled -= localTankInput.OnAim;
            m_inputActions.Player.Dash.performed -= localTankInput.OnDash;
            //m_inputActions.Player.Dash.canceled -= predictedController.OnDash;
            m_inputActions.Player.Parry.performed -= localTankInput.OnParry;
            m_inputActions.Player.Parry.canceled -= localTankInput.OnParry;
            m_inputActions.Player.Fire.performed -= localTankInput.OnFire;
            m_inputActions.Player.Fire.canceled -= localTankInput.OnFire;
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
            //Debug.Log("Starting client clocks...");
            SimClock.Instance.StartClock();
            AutoPhysics2DUpdate(false);
        }

        public void SetObjectPosition(GameObject targetObject, Vector3 newPosition, Quaternion newRotation)
        {
            NetworkObjectReference networkObjectReference = new NetworkObjectReference(targetObject);
            SetTankPositionClientRpc(networkObjectReference, newPosition, newRotation);
        }

        [ClientRpc]
        private void SetTankPositionClientRpc(NetworkObjectReference targetObjectReference, Vector3 newPosition, Quaternion newRotation)
        {
            if (targetObjectReference.TryGet(out var targetObject))
            {
                GameObject target = targetObject.GetComponent<TankSimulationObject>().TankController.gameObject;

                if (target != null)
                {
                    target.gameObject.GetComponent<Transform>().position = newPosition;
                    target.gameObject.GetComponent<Transform>().rotation = newRotation;
                    //Debug.Log($"GameObject del jugador {targetObject.GetComponent<NetworkObject>().OwnerClientId} colocado en el punto {newPosition.ToString()}");
                }
                else
                {
                    Debug.LogWarning($"No se encontró el NetworkObject para el cliente con ID {targetObject.GetComponent<NetworkObject>().OwnerClientId}");
                }
            }
        }

        [ClientRpc]
        public void SetTankPositionClientRpc(NetworkObjectReference targetObjectReference, Vector3 newPosition, Quaternion newRotation, ulong clientId)
        {
            if (NetworkManager.Singleton.LocalClientId == clientId)
            {
                if (targetObjectReference.TryGet(out var targetObject))
                {
                    GameObject target = targetObject.GetComponent<TankSimulationObject>().TankController.gameObject;

                    if (target != null)
                    {
                        target.gameObject.GetComponent<Transform>().position = newPosition;
                        target.gameObject.GetComponent<Transform>().rotation = newRotation;
                        //Debug.Log($"GameObject del jugador {targetObject.GetComponent<NetworkObject>().OwnerClientId} colocado en el punto {newPosition.ToString()}");
                    }
                    else
                    {
                        Debug.LogWarning($"No se encontró el NetworkObject para el cliente con ID {targetObject.GetComponent<NetworkObject>().OwnerClientId}");
                    }
                }
            }
        }
    }
}