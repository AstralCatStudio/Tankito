using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class RoundManager : NetworkBehaviour
{
    private int _currentRound = 0;
    public int _maxRounds = 5;

    private List<GameObject> _players = new List<GameObject>();
    private List<GameObject> _alivePlayer;

    GameObject prueba; // Prueba

    void Start()
    {
        // Prueba
        prueba = new GameObject();
        AddPlayer(prueba);
        AddPlayer(new GameObject());
        //

        //if (IsServer)
        //{
            InitializeRound();
        //}
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            EliminatePlayer(prueba);
        }
    }

    public void AddPlayer(GameObject player)
    {
        _players.Add(player);
        Debug.Log("nº jugadores: " + _players.Count);
    }

    public void EliminatePlayer(GameObject player)
    {
        _alivePlayer.Remove(player);
        CheckForWinner();
    }

    private void InitializeRound()
    {
        _alivePlayer = new List<GameObject>(_players);
        _currentRound++;
        Debug.Log("Inicio ronda " + _currentRound);
    }

    private void CheckForWinner()
    {
        if (_alivePlayer.Count == 1)
        {
            Debug.Log("Alguien ha ganado la ronda");
            EndRound();
        }
        else
        {
            Debug.Log("La ronda sigue");
        }
    }

    private void EndRound()
    {
        Debug.Log("Fin de ronda");
        ShowRanking(); // Prueba
        ShowRankingClientRpc();
        Invoke(nameof(CheckRoundsCount), 5.0f);
    }

    private void ShowRanking() // Prueba
    {
        Debug.Log("Se muestra el ranking");
    }

    [ClientRpc]
    private void ShowRankingClientRpc()
    {
        Debug.Log("Se muestra el ranking en todos");
    }

    private void CheckRoundsCount()
    {
        if (_currentRound < _maxRounds)
        {
            PowerUpSelection();
            Invoke(nameof(InitializeRound),5.0f);
        }
        else
        {
            EndGame();
        }
    }

    private void PowerUpSelection()
    {
        Debug.Log("Se eligen power ups");
        ShowPowerUpsClientRpc();
    }

    [ClientRpc]
    private void ShowPowerUpsClientRpc()
    {
        Debug.Log("Se muestran los power ups en todos");
    }

    private void EndGame()
    {
        Debug.Log("Fin de la partida");
        EndGameClientRpc();
    }

    [ClientRpc]
    private void EndGameClientRpc()
    {
        Debug.Log("Final de partida en todos");
    }
}
