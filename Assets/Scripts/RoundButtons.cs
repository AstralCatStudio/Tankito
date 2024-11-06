using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class RoundButtons : NetworkBehaviour
{
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _readyButton;

    RoundManager _roundManager;

    private NetworkList<bool> _readyCount;

    private bool _canStart;

    private void Awake()
    {
        _canStart = false;
        _readyCount = new NetworkList<bool>();
    }

    private void Update()
    {
        PrintReadyCount();
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
        _readyCount[(int)clientId-1] = !_readyCount[(int)clientId-1];
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
        }
    }

    private void PrintReadyCount()
    {
        int readyCount = 0;
        foreach (var ready in _readyCount)
        {
            if (ready)
            {
                readyCount++;
            }
        }
        Debug.Log($"Listos: {readyCount}");
    }

    #endregion

    #region Start

    private void OnStartClicked()
    {
        Debug.Log("Pulsaste Start");
        if(IsServer)
        {
            if(_canStart)
            {
                _roundManager.InitializeRound();
            }
            else
            {
                Debug.Log("No hay suficientes listos");
            }
        }
    }

    #endregion
}
