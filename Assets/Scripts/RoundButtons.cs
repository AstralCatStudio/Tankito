using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoundButtons : NetworkBehaviour
{
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _readyButton;
    [SerializeField] private TMP_Text _readyPlayersText;
    [SerializeField] private GameObject _notReadyMessage;
    [SerializeField] private TMP_Text _readyLocalText;

    private RoundManager _roundManager;

    [Serializable]
    private struct PlayerReadyStatus: INetworkSerializable, IEquatable<PlayerReadyStatus>
    {
        public ulong ClientId;
        public bool IsReady;

        public PlayerReadyStatus(ulong clientId, bool isReady)
        {
            ClientId = clientId;
            IsReady = isReady;
        }

        public bool Equals(PlayerReadyStatus other)
        {
            throw new NotImplementedException();
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref IsReady);
        }
    }

    private NetworkList<PlayerReadyStatus> _readyStatusList;

    private bool _canStart;

    private void Awake()
    {
        _canStart = false;
        _readyStatusList = new NetworkList<PlayerReadyStatus>();

        _readyPlayersText.gameObject.SetActive(true);
        UpdateReadyPlayersText();
    }

    private void Update()
    {
        Debug.Log($"Clientes listos en la lista: {CalcReadyCount()} {_readyStatusList.Count}");

    }

    private void Start()
    {
        if (IsServer)
        {
            _startButton.gameObject.SetActive(true);
            _startButton.onClick.AddListener(OnStartClicked);

            _roundManager = FindObjectOfType<RoundManager>();
            if (_roundManager == null)
            {
                Debug.Log("No se encontro RM");
            }
            else
            {
                Debug.Log("RM encontrado SUUUU");
            }
        }
        else if (IsClient && !IsServer)
        {
            _readyButton.gameObject.SetActive(true);
            _readyButton.onClick.AddListener(OnReadyClicked);

            _readyLocalText.gameObject.SetActive(true);
        }
    }
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Debug.Log("Entro en network spawn");
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
        _readyStatusList.OnListChanged += OnReadyCountChanged;
    }

    public override void OnDestroy()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        _readyStatusList.OnListChanged -= OnReadyCountChanged;
    }

    #region ClientConnection
    private void OnClientConnected(ulong clientId)
    {
        _readyStatusList.Add(new PlayerReadyStatus { ClientId = clientId, IsReady = false});
    }

    private void OnClientDisconnected(ulong clientId)
    {
        for(int i = 0;  i < _readyStatusList.Count; i++)
        {
            if (_readyStatusList[i].ClientId == clientId)
            {
                _readyStatusList.RemoveAt(i);
                break;
            }
        }
    }

    #endregion

    #region Ready
    private void OnReadyClicked()
    {
        Debug.Log("Pulsaste Ready");
        SetPlayerReadyServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ulong clientId)
    {
        // Cambia el estado de "Listo" del cliente en la lista
        for(int i = 0; i < _readyStatusList.Count; i++)
        {
            if (_readyStatusList[i].ClientId == clientId)
            {
                var status = _readyStatusList[i];
                status.IsReady = !status.IsReady;
                _readyStatusList[i] = status;
                break;
            }
        }
    }

    private void OnReadyCountChanged(NetworkListEvent<PlayerReadyStatus> readyCountChanged)
    {
        if (IsServer)
        {
            bool allReady = true;

            foreach (var status in _readyStatusList)
            {
                if (!status.IsReady)
                {
                    allReady = false;
                    break;
                }
            }

            _canStart = allReady;

            if (_canStart)
            {
                _notReadyMessage.SetActive(false);
            }
        }

        UpdateReadyPlayersText();
        UpdateLocalReadyText();
    }

    private int CalcReadyCount()
    {
        int readyCount = 0;
        foreach (var status in _readyStatusList)
        {
            if (status.IsReady)
            {
                readyCount++;
            }
        }
        return readyCount;
    }

    private void UpdateReadyPlayersText()
    {
        _readyPlayersText.text = $"Ready players: {CalcReadyCount()} / {_readyStatusList.Count}";
    }

    private void UpdateLocalReadyText()
    {
        if(!IsServer)
        {
            ulong clientId = NetworkManager.Singleton.LocalClientId;
            foreach (var status in _readyStatusList)
            {
                if(status.ClientId == clientId)
                {
                    _readyLocalText.text = status.IsReady ? "Ready!" : "Not Ready";
                    Debug.Log(status.IsReady ? "Ready!" : "Not Ready");
                    break;
                }
            }
        }
    }

    #endregion

    #region Start

    private void OnStartClicked()
    {
        Debug.Log("Pulsaste Start");
        if (IsServer)
        {
            if (_canStart)
            {
                _roundManager.InitializeRound();
                DestroyButtonsClientRpc();
            }
            else
            {
                Debug.Log("No hay suficientes listos");
                _notReadyMessage.SetActive(true);
            }
        }
    }

    [ClientRpc]
    private void DestroyButtonsClientRpc()
    {
        Destroy(gameObject);
    }

    #endregion
}
