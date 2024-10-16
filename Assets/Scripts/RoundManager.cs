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
        //prueba = new GameObject();
        //AddPlayer(prueba);
        //AddPlayer(new GameObject());
        //

        if (IsServer)
        {
            //InitializeRound();
        }
    }

    void Update()
    {
        if (IsServer)
        {
            if (Input.GetKeyUp(KeyCode.Space))
            {
                _alivePlayer[1].GetComponent<TankData>().TakeDamage(2);
            }

            if (Input.GetKeyUp(KeyCode.Alpha1))
            {
                InitializeRound();
            }

        }
        //if (Input.GetKeyUp(KeyCode.Space))
        //{
        //    EliminatePlayer(prueba);
        //}
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

        if (_alivePlayer.Contains(player))
        {
            _alivePlayer.Remove(player);
            Debug.Log($"El jugador {player.name} ha sido eliminado");
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
    #endregion

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
        Debug.Log("NETLESS: Fin de ronda");
        EndRoundClientRpc();
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
            //ShowRankingClientRpc();
            Invoke(nameof(PowerUpSelection), 3.0f);
            Invoke(nameof(InitializeRound), 8.0f);
        }
        else
        {
            ShowFinalRanking();
            //ShowFinalRankingClientRpc();
            Invoke(nameof(EndGame), 5.0f);
        }
    }

    private void ShowRanking()
    {
        Debug.Log("NETLESS: Se muestra el ranking");
        ShowRankingClientRpc();
    }

    [ClientRpc]
    private void ShowRankingClientRpc()
    {
        Debug.Log("NETCODE: Se muestra el ranking en todos");
    }

    private void ShowFinalRanking()
    {
        Debug.Log("NETLESS: Se muestra el ranking final");
        ShowFinalRankingClientRpc();
    }

    [ClientRpc]
    private void ShowFinalRankingClientRpc()
    {
        Debug.Log("NETCODE: Se muestra el ranking final en todos");
    }

    private void PowerUpSelection()
    {
        Debug.Log("NETLESS: Se eligen power ups");
        ShowPowerUpsClientRpc();
    }

    [ClientRpc]
    private void ShowPowerUpsClientRpc()
    {
        Debug.Log("NETCODE: Se muestran los power ups en todos");
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
}
