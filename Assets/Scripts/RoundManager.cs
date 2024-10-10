using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class RoundManager : NetworkBehaviour
{
    private int _currentRound = 1;
    public int _maxRounds = 5;

    private List<GameObject> _players = new List<GameObject>();
    //public GameObject _uiPowerUps;

    private enum RoundState
    {
        Battle,             // Tanques pelean
        Ranking,            // Se muestra la clasificacion
        PowerUpSelection,   // Seleccion de power ups
        GameOver            // Fin de la partida (alguien gana)
    }

    private RoundState _currentState;

    void Start()
    {
        //if (IsServer)
        //{
        this.StartRound();
        //}
    }

    void Update()
    {
        //if (!IsServer) return;  // Solo maneja las rondas el server

        switch (_currentState)
        {
            case RoundState.Battle:
                // Los tanques pelean hasta que solo queda uno vivo (ganador)
                CheckWinner();
                break;

            case RoundState.Ranking:
                // Se muestra el ranking en pantalla
                break;

            case RoundState.PowerUpSelection:
                // Se seleccionan los power ups
                break;

            case RoundState.GameOver:
                // Se finaliza la partida: ranking, recompensas, etc.
                break;
        }
    }

    #region RoundStart

    private void StartRound()
    {
        Debug.Log("Comienza la ronda " + _currentRound);

        this.ResetPlayers();

        _currentState = RoundState.Battle;
    }

    private void ResetPlayers()
    {
        Debug.Log("Reseteo jugadores");
        for (int i = 0; i < _players.Count; i++)
        {
            // Se resetea la vida del jugador
            // Se establece su spawn (supongo)
            Debug.Log("Reseteo jugador " + i);
        }
    }

    #endregion

    #region InsertPlayers

    public void AddPlayer(GameObject player)
    {
        Debug.Log("Se anade jugador");
        _players.Add(player);
    }

    public void DeletePlayer(GameObject player) // En caso de querer gestionar desconexiones supongo
    {
        _players.Remove(player);
    }

    #endregion

    #region StateActions

    private void CheckWinner()
    {
        int remainingPlayers = 1;

        if (remainingPlayers == 1)
        {
            Debug.Log("Alguien ha ganado");
            EndRound();
        }
    }

    private void EndRound()
    {
        Debug.Log("Fin de ronda");
        if (_currentRound == _maxRounds)
        {
            _currentState = RoundState.GameOver;
        }
        else
        {
            _currentState = RoundState.Ranking;
        }

    }

    #endregion

}
