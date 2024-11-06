using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor.PackageManager;

public class RoundButtons : NetworkBehaviour
{
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _readyButton;
    [SerializeField] private TMP_Text _readyPlayersText;
    [SerializeField] private GameObject _notReadyMessage;
    [SerializeField] private TMP_Text _readyLocalText;

    private RoundManager _roundManager;

    private NetworkList<bool> _readyCount;

    private bool _canStart;

    private void Awake()
    {
        _canStart = false;
        _readyCount = new NetworkList<bool>();

        _readyPlayersText.gameObject.SetActive(true);
        UpdateReadyPlayersText();
    }

    private void Update()
    {
        Debug.Log($"Clientes listos en la lista: {CalcReadyCount()} {_readyCount.Count}");

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
        _readyCount.OnListChanged += OnReadyCountChanged;
    }

    public override void OnDestroy()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        _readyCount.OnListChanged -= OnReadyCountChanged;
    }

    #region ClientConnection
    private void OnClientConnected(ulong clientId)
    {
        _readyCount.Add(false);
    }

    private void OnClientDisconnected(ulong clientId)
    {
        _readyCount.RemoveAt((int)clientId);
    }

    #endregion

    #region Ready
    private void OnReadyClicked()
    {
        Debug.Log("Pulsaste Ready");
        SetPlayerReadyServerRpc(NetworkManager.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ulong clientId)
    {
        // Cambia el estado de "Listo" del cliente en la lista
        _readyCount[(int)clientId - 1] = !_readyCount[(int)clientId - 1];
    }

    private void OnReadyCountChanged(NetworkListEvent<bool> readyCountChanged)
    {
        if (IsServer)
        {
            bool allReady = true;

            foreach (var ready in _readyCount)
            {
                if (!ready)
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
        foreach (var ready in _readyCount)
        {
            if (ready)
            {
                readyCount++;
            }
        }
        return readyCount;
    }

    private void UpdateReadyPlayersText()
    {
        _readyPlayersText.text = $"Ready players: {CalcReadyCount()} / {_readyCount.Count}";
    }

    private void UpdateLocalReadyText()
    {
        if(!IsServer)
        {
            if (_readyCount[(int)NetworkManager.LocalClientId - 1])
            {
                _readyLocalText.text = "Ready!";
                Debug.Log("Ready");
            }
            else
            {
                _readyLocalText.text = "Not Ready";
                Debug.Log("Not Ready");
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
