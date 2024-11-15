using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class RoundManager : NetworkBehaviour
{
    private int _currentRound = 0;
    public int _maxRounds = 5;

    private string _ranking;

    public RoundUI _roundUI;

    public float _countdownTime = 5f;
    public TMP_Text _countdownText;
    private float _currentTime;
    private bool _isCountingDown = false;

    private Dictionary<ulong, GameObject> _players = new Dictionary<ulong, GameObject>();
    private Dictionary<ulong, GameObject> _alivePlayers = new Dictionary<ulong, GameObject>();

    public bool _startedGame;
    private bool _startedRound;

    private GameObject _playerInput;

    private SpawnManager _spawnManager;

    void Start()
    {
        _startedGame = false;
        _startedRound = false;

        _playerInput = GameObject.Find("PlayerInput");

        if (IsServer)
        {
            _spawnManager = GameObject.Find("SpawnManager").GetComponent<SpawnManager>();
        }

        if (!_startedGame)
        {
            DisablePlayerInput();
        }

        _roundUI = FindObjectOfType<RoundUI>();

        _countdownText = GameObject.Find("Countdown").GetComponentInChildren<TMP_Text>();
    }

    void Update()
    {
        if (IsServer)
        {
            if (Input.GetKeyUp(KeyCode.Alpha2) && _startedRound && _alivePlayers.Count > 1)
            {
                DebugDamagePlayer();
            }

        }

        if (_isCountingDown)
        {
            _currentTime -= Time.deltaTime;

            if (_countdownText != null)
            {
                string newText = Mathf.Ceil(_currentTime).ToString();
                SetCountdownTextClientRpc(newText);
            }

            if (_currentTime <= 0f)
            {
                _currentTime = 0f;
                EndCountdown();
            }

        }

    }

    #region PlayerManagement
    public void AddPlayer(GameObject player)
    {
        ulong clientId = player.GetComponent<NetworkObject>().OwnerClientId;
        if (!_players.ContainsKey(clientId))
        {
            _players.Add(clientId, player);
            Debug.Log($"Jugador anadido. Nº de jugadores: {_players.Count}");
        }
    }
    
    public void RemovePlayer(ulong clientId)
    {
        if (_players.ContainsKey(clientId))
        {
            _players.Remove(clientId);
            if(_startedGame && _alivePlayers.ContainsKey(clientId))
            {
                _alivePlayers.Remove(clientId);
            }

            Debug.Log($"Jugador desconectado y eliminado. Nº de jugadores: {_players.Count}");

            UpdateRemainingPlayersTextClientRpc(_alivePlayers.Count);
            CheckForWinner();
        }
    }

    public void EliminatePlayer(GameObject player)
    {
        if (!IsServer) return;

        ulong clientId = player.GetComponent<NetworkObject>().OwnerClientId;

        if (_alivePlayers.ContainsKey(clientId))
        {
            _alivePlayers.Remove(clientId);

            DisablePlayerInputClientRpc(player.GetComponent<NetworkObject>().OwnerClientId);

            NetworkObjectReference playerReference = new NetworkObjectReference(player.GetComponent<NetworkObject>());
            DisableTankClientRpc(playerReference);

            player.GetComponent<TankData>().IncreasePoints(_players.Count - _alivePlayers.Count);

            Debug.Log($"El jugador {player.GetComponent<NetworkObject>().OwnerClientId} de puntuacion {player.GetComponent<TankData>().points} ha sido eliminado");

            UpdateRemainingPlayersTextClientRpc(_alivePlayers.Count);
            CheckForWinner();
        }
    }

    private void OnEnable()
    {
        Debug.Log("Se suscribe al evento de morir tanque");
        TankData.OnTankDestroyed += EliminatePlayer;
    }

    private void OnDisable()
    {
        Debug.Log("Se desuscribe al evento de morir tanque");
        TankData.OnTankDestroyed -= EliminatePlayer;
    }

    public void DebugDamagePlayer()
    {
        _alivePlayers.ElementAt(1).Value.GetComponent<TankData>().TakeDamage(2);
    }

    [ClientRpc]
    private void DisableTankClientRpc(NetworkObjectReference targetObjectReference)
    {
        if (targetObjectReference.TryGet(out var targetObject))
        {
            if (targetObject != null)
            {
                Debug.Log($"GameObject del jugador {targetObject.GetComponent<NetworkObject>().OwnerClientId} desactivado en todos los clientes.");
                targetObject.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning($"No se encontró el PlayerObject para el cliente con ID {targetObject.GetComponent<NetworkObject>().OwnerClientId}");
            }
        }
    }

    /*
    [ClientRpc]
    private void SetObjectPositionClientRpc(NetworkObjectReference targetObjectReference, Vector3 newPosition)
    {
        if (targetObjectReference.TryGet(out var targetObject))
        {
            if (targetObject != null)
            {
                targetObject.gameObject.GetComponent<Transform>().position = newPosition;
                Debug.Log($"GameObject del jugador {targetObject.GetComponent<NetworkObject>().OwnerClientId} colocado en el punto {newPosition.ToString()}");
            }
            else
            {
                Debug.LogWarning($"No se encontró el PlayerObject para el cliente con ID {targetObject.GetComponent<NetworkObject>().OwnerClientId}");
            }
        }
    }*/

    [ClientRpc]
    private void EnableTankClientRpc(NetworkObjectReference targetObjectReference)
    {
        if (targetObjectReference.TryGet(out var targetObject))
        {
            if (targetObject != null)
            {
                Debug.Log($"GameObject del jugador {targetObject.GetComponent<NetworkObject>().OwnerClientId} activado en todos los clientes.");
                targetObject.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"No se encontró el PlayerObject para el cliente con ID {targetObject.GetComponent<NetworkObject>().OwnerClientId}");
            }
        }
    }

    #endregion

    public void InitializeRound()
    {
        _roundUI.SetActivePowerUps(false);
        DisablePowerUpsClientRpc();

        _currentRound++;

        ResetPlayers();

        Debug.Log("Inicio ronda " + _currentRound);
        UpdateRemainingPlayersTextClientRpc(_alivePlayers.Count);
        StartCountdown();
    }

    public bool IsGameStarted()
    {
        return _startedGame;
    }

    private void ResetPlayers()
    {
        _alivePlayers = new Dictionary<ulong, GameObject>(_players);

        if (_currentRound > 1)
        {
            _spawnManager.ResetSpawnPoints();

            foreach (var aux in _alivePlayers)
            {
                GameObject player = aux.Value;
                if (player != null)
                {
                    if (!player.activeSelf)
                    {
                        NetworkObjectReference targetObject = new NetworkObjectReference(player.GetComponent<NetworkObject>());
                        EnableTankClientRpc(targetObject);
                    }
                    player.GetComponent<TankData>().ResetTank();
                }
            }
        }
    }

    [ClientRpc]
    private void SetActiveRemainingPlayersClientRpc(bool active)
    {
        _roundUI.SetRemainingPlayersActive(active);
    }

    [ClientRpc]
    private void UpdateRemainingPlayersTextClientRpc(int players)
    {
        Debug.Log("Rpc player text");
        _roundUI.SetRemainingPlayers(players);
    }

    #region Countdown

    private void StartCountdown()
    {
        _currentTime = _countdownTime;
        _isCountingDown = true;

        Debug.Log("Cuenta atras iniciada");

        DisablePlayerInputClientRpc();
    }

    private void EndCountdown()
    {
        _isCountingDown = false;

        Debug.Log("Fin de cuenta atras");

        SetCountdownTextClientRpc("BATTLE!");

        _startedRound = true;

        SetActiveRemainingPlayersClientRpc(true);

        EnablePlayerInputClientRpc();
    }

    [ClientRpc]
    private void SetCountdownTextClientRpc(string text)
    {
        _startedGame = true;

        if (_countdownText != null)
        {
            _countdownText.text = text;
        }
    }

    #endregion

    #region PlayerInputManagement
    private void DisablePlayerInput()
    {
        _playerInput.SetActive(false);
    }

    [ClientRpc]
    private void DisablePlayerInputClientRpc()
    {
        if (_playerInput != null)
        {
            Debug.Log("Player input desactivado");
            _playerInput.SetActive(false);
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
            if (_playerInput != null)
            {
                Debug.Log("Player input desactivado");
                _playerInput.SetActive(false);
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
        if (_playerInput != null)
        {
            _playerInput.SetActive(true);
        }
    }

    #endregion

    #region FlujoPartida

    [ClientRpc]
    private void DisablePowerUpsClientRpc()
    {
        _roundUI.SetActivePowerUps(false);
    }

    private void CheckForWinner()
    {
        if (_alivePlayers.Count == 1)
        {
            Debug.Log("Alguien ha ganado la ronda");
            _alivePlayers[0].GetComponent<TankData>().IncreasePoints(_players.Count);
            EndRound();
        }
        else
        {
            Debug.Log("La ronda sigue");
        }
    }

    private void EndRound()
    {
        Debug.Log("NETLESS: Fin de ronda");
        _startedRound = false;
        EndRoundClientRpc();
        DisablePlayerInputClientRpc();
        SetActiveRemainingPlayersClientRpc(false);
        BetweenRounds();
    }

    [ClientRpc]
    private void EndRoundClientRpc()
    {
        Debug.Log("NETCODE: Fin de ronda en todos");
    }

    private void BetweenRounds()
    {
        if (_currentRound < _maxRounds)
        {
            ShowRanking();
            Invoke(nameof(PowerUpSelection), 3.0f);
            Invoke(nameof(InitializeRound), 8.0f);
        }
        else
        {
            ShowRanking(); //ShowFinalRanking();
            //ShowFinalRankingClientRpc();
            Invoke(nameof(EndGame), 5.0f);
        }
    }

    private void ShowRanking()
    {
        Debug.Log("NETLESS: Se muestra el ranking");
        GenerateRanking();
        ShowRankingClientRpc(_ranking);
    }

    [ClientRpc]
    private void ShowRankingClientRpc(string ranking)
    {
        Debug.Log("NETCODE: Se muestra el ranking en todos");
        _roundUI.SetActiveRanking(true);
        _roundUI.SetRankingText(ranking);
    }

    private void ShowFinalRanking()
    {
        Debug.Log("NETLESS: Se muestra el ranking final");
        //_roundUI.SetActiveRankingFinal(true);
        ShowFinalRankingClientRpc();
    }

    [ClientRpc]
    private void ShowFinalRankingClientRpc()
    {
        Debug.Log("NETCODE: Se muestra el ranking final en todos");
        _roundUI.SetActiveRankingFinal(true);
    }

    private void PowerUpSelection()
    {
        Debug.Log("NETLESS: Se eligen power ups");
        // Aqui se meteria la logica de elegir power ups supongo
        ShowPowerUpsClientRpc();
    }

    [ClientRpc]
    private void ShowPowerUpsClientRpc()
    {
        Debug.Log("NETCODE: Se muestran los power ups en todos");
        _roundUI.SetActiveRanking(false);
        _roundUI.SetActivePowerUps(true);
    }

    private void EndGame()
    {
        Debug.Log("NETLESS: Fin de la partida");
        EndGameClientRpc();
    }

    [ClientRpc]
    private void EndGameClientRpc()
    {
        Debug.Log("NETCODE: Final de partida en todos");
    }
    #endregion

    private void GenerateRanking()
    {
        if (_currentRound == _maxRounds)
        {
            _ranking = "Ranking Final: ";
        }
        else
        {
            _ranking = "Ranking: ";
        }

        List<GameObject> sortedPlayers = _players.Values.OrderByDescending(player => player.GetComponent<TankData>().points).ToList<GameObject>();

        for (int i = 0; i < sortedPlayers.Count; i++)
        {
            _ranking += $"\n{i + 1}. Jugador {sortedPlayers[i].GetComponent<NetworkObject>().OwnerClientId}:  {sortedPlayers[i].GetComponent<TankData>().points} puntos";
        }
    }
}
