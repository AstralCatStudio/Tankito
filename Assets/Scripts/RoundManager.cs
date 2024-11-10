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

    private List<GameObject> _players = new List<GameObject>();
    private List<GameObject> _alivePlayers;

    private bool _startedGame;

    private GameObject _playerInput;

    void Start()
    {
        _startedGame = false;

        _playerInput = GameObject.Find("PlayerInput");

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
            /*if (Input.GetKeyUp(KeyCode.Alpha1) && !_startedGame && _players.Count > 1)
            {
                InitializeRound();
            }*/

            if (Input.GetKeyUp(KeyCode.Alpha2) && _alivePlayers.Count > 1)
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
        _players.Add(player);
        Debug.Log("Jugador añadido. Nº jugadores: " + _players.Count);
    }

    public void EliminatePlayer(GameObject player)
    {
        if (!IsServer) return;

        if (_alivePlayers.Contains(player))
        {
            _alivePlayers.Remove(player);
            DisablePlayerInputClientRpc(player.GetComponent<NetworkObject>().OwnerClientId);
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
        _alivePlayers[1].GetComponent<TankData>().TakeDamage(2);
    }
    #endregion

    public void InitializeRound()
    {
        _roundUI.SetActivePowerUps(false);
        DisablePowerUpsClientRpc();

        _alivePlayers = new List<GameObject>(_players);
        _currentRound++;
        if (_currentRound > 1)
        {
            for (int i = 0; i < _alivePlayers.Count; i++)
            {
                _alivePlayers[i].GetComponent<TankData>().Reset();
            }
        }

        Debug.Log("Inicio ronda " + _currentRound);
        UpdateRemainingPlayersTextClientRpc(_alivePlayers.Count);
        StartCountdown();
    }

    [ClientRpc]
    private void UpdateRemainingPlayersTextClientRpc(int players)
    {
        Debug.Log("Rpc player text");
        _roundUI.SetPlayersAlive(players);
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
        EndRoundClientRpc();
        DisablePlayerInputClientRpc();
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

        List<GameObject> sortedPlayers = _players.OrderByDescending(player => player.GetComponent<TankData>().points).ToList<GameObject>();

        for (int i = 0; i < sortedPlayers.Count; i++)
        {
            _ranking += $"\n{i + 1}. Jugador {sortedPlayers[i].GetComponent<NetworkObject>().OwnerClientId}:  {sortedPlayers[i].GetComponent<TankData>().points} puntos";
        }
    }
}
