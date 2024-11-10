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

    public GameObject _playerInput;
    [SerializeField] private bool DEBUG = false;

    void Start()
    {
        //if (!_startedGame)
        //{
        //    DisablePlayerInput();
        //}

        _startedGame = false;

        _roundUI = FindObjectOfType<RoundUI>();

        _countdownText = GameObject.Find("Countdown").GetComponentInChildren<TMP_Text>();
    }

    void Update()
    {
        if (IsServer)
        {
            if (Input.GetKeyUp(KeyCode.Alpha1) && !_startedGame && _players.Count > 1)
            {
                InitializeRound();
            }

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
                _countdownText.text = newText;
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
        if (DEBUG) Debug.Log("Jugador a�adido. N� jugadores: " + _players.Count);
    }

    public void EliminatePlayer(GameObject player)
    {
        if (!IsServer) return;

        if (_alivePlayers.Contains(player))
        {
            _alivePlayers.Remove(player);
            player.GetComponent<TankData>().IncreasePoints(_players.Count - _alivePlayers.Count);
            if (DEBUG) Debug.Log($"El jugador {player.name} de puntuacion {player.GetComponent<TankData>().points} ha sido eliminado");
            UpdateRemainingPlayersText();
            CheckForWinner();
        }
    }

    private void OnEnable()
    {
        if (DEBUG) Debug.Log("Se suscribe al evento de morir tanque");
        TankData.OnTankDestroyed += EliminatePlayer;
    }

    private void OnDisable()
    {
        if (DEBUG) Debug.Log("Se desuscribe al evento de morir tanque");
        TankData.OnTankDestroyed -= EliminatePlayer;
    }

    public void DebugDamagePlayer()
    {
        _alivePlayers[1].GetComponent<TankData>().TakeDamage(2);
    }
    #endregion

    private void InitializeRound()
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

        if (DEBUG) Debug.Log("Inicio ronda " + _currentRound);
        UpdateRemainingPlayersText();
        StartCountdown();
    }

    private void UpdateRemainingPlayersText()
    {
        _roundUI.SetPlayersAlive(_alivePlayers.Count);
        UpdateRemainingPlayersTextClientRpc(_alivePlayers.Count);
    }

    [ClientRpc]
    private void UpdateRemainingPlayersTextClientRpc(int players)
    {
        _roundUI.SetPlayersAlive(players);
    }

    #region Countdown

    private void StartCountdown()
    {
        _currentTime = _countdownTime;
        _isCountingDown = true;

        if (DEBUG) Debug.Log("Cuenta atras iniciada");

        DisablePlayerInput();
    }

    private void EndCountdown()
    {
        _isCountingDown = false;

        if (DEBUG) Debug.Log("Fin de cuenta atras");
        _countdownText.text = "BATTLE!";
        SetCountdownTextClientRpc("BATTLE!");
        _startedGame = true;
        EnablePlayerInput();
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
        DisablePlayerInputClientRpc();
    }

    [ClientRpc]
    private void DisablePlayerInputClientRpc()
    {
        if (_playerInput != null)
        {
            _playerInput.SetActive(false);
        }
    }

    private void EnablePlayerInput()
    {
        _playerInput.SetActive(true);
        EnablePlayerInputClientRpc();
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
            if (DEBUG) Debug.Log("Alguien ha ganado la ronda");
            _alivePlayers[0].GetComponent<TankData>().IncreasePoints(_players.Count);
            EndRound();
        }
        else
        {
            if (DEBUG) Debug.Log("La ronda sigue");
        }
    }

    private void EndRound()
    {
        if (DEBUG) Debug.Log("NETLESS: Fin de ronda");
        EndRoundClientRpc();
        DisablePlayerInput();
        BetweenRounds();
    }

    [ClientRpc]
    private void EndRoundClientRpc()
    {
        if (DEBUG) Debug.Log("NETCODE: Fin de ronda en todos");
    }

    private void BetweenRounds()
    {
        if (_currentRound < _maxRounds)
        {
            ShowRanking();
            //ShowRankingClientRpc();
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
        if (DEBUG) Debug.Log("NETLESS: Se muestra el ranking");
        GenerateRanking();
        _roundUI.SetActiveRanking(true);
        _roundUI.SetRankingText(_ranking);
        ShowRankingClientRpc(_ranking);
    }

    [ClientRpc]
    private void ShowRankingClientRpc(string ranking)
    {
        if (DEBUG) Debug.Log("NETCODE: Se muestra el ranking en todos");
        _roundUI.SetActiveRanking(true);
        _roundUI.SetRankingText(ranking);
    }

    private void ShowFinalRanking()
    {
        if (DEBUG) Debug.Log("NETLESS: Se muestra el ranking final");
        _roundUI.SetActiveRankingFinal(true);
        ShowFinalRankingClientRpc();
    }

    [ClientRpc]
    private void ShowFinalRankingClientRpc()
    {
        if (DEBUG) Debug.Log("NETCODE: Se muestra el ranking final en todos");
        _roundUI.SetActiveRankingFinal(true);
    }

    private void PowerUpSelection()
    {
        if (DEBUG) Debug.Log("NETLESS: Se eligen power ups");
        _roundUI.SetActiveRanking(false);
        _roundUI.SetActivePowerUps(true);
        ShowPowerUpsClientRpc();
    }

    [ClientRpc]
    private void ShowPowerUpsClientRpc()
    {
        if (DEBUG) Debug.Log("NETCODE: Se muestran los power ups en todos");
        _roundUI.SetActiveRanking(false);
        _roundUI.SetActivePowerUps(true);
    }

    private void EndGame()
    {
        if (DEBUG) Debug.Log("NETLESS: Fin de la partida");
        EndGameClientRpc();
    }

    [ClientRpc]
    private void EndGameClientRpc()
    {
        if (DEBUG) Debug.Log("NETCODE: Final de partida en todos");
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
            _ranking += $"\n{i + 1}. {sortedPlayers[i].name}  {sortedPlayers[i].GetComponent<TankData>().points} puntos";
        }
    }
}
