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

    void Start()
    {
        //if (IsServer)
        //{
        InitializeRound();
        //}
    }

    public void AddPlayer(GameObject player)
    {
        _players.Add(player);
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
        if(_alivePlayer.Count == 1)
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
        Debug.Log("Se pasa al ranking y despues a otra ronda");
        /*ShowRanking();
        PowerUpSelection();
        ResetRound();*/
        ShowRankingClientRpc();
        Invoke(nameof(PowerUpSelection), 5.0f);
    }

    [ClientRpc]
    private void ShowRankingClientRpc()
    {
        Debug.Log("Se muestra el ranking en todos");
    }

    /*private void ShowRanking()
    {
        Debug.Log("Se muestra el ranking");
    }*/

    private void PowerUpSelection()
    {
        Debug.Log("Se eligen power ups");
        ShowPowerUpsClientRpc();
        ResetRound();
    }

    [ClientRpc]
    private void ShowPowerUpsClientRpc()
    {
        Debug.Log("Se muestran los power ups en todos");
    }

    private void ResetRound()
    {
        if(_currentRound < _maxRounds)
        {
            InitializeRound();
        }
        else
        {
            EndGame();
        }
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
