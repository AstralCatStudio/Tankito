using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class RoundManager : NetworkBehaviour
{
    private int _currentRound = 0;
    public int _maxRounds = 5;

    public RoundUI _roundUI;

    private List<GameObject> _players = new List<GameObject>();
    private List<GameObject> _alivePlayers;

    GameObject prueba; // Prueba

    void Start()
    {
        _roundUI = FindObjectOfType<RoundUI>();

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
                _alivePlayers[1].GetComponent<TankData>().TakeDamage(2);
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

        if (_alivePlayers.Contains(player))
        {
            _alivePlayers.Remove(player);
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
        _roundUI.SetActivePowerUps(false);
        DisablePowerUpsClientRpc();
        _alivePlayers = new List<GameObject>(_players);
        _currentRound++;
        if( _currentRound > 1 )
        {
            for(int i = 0; i < _alivePlayers.Count; i++)
            {
                _alivePlayers[i].GetComponent<TankData>().Reset();
            }
        }
        Debug.Log("Inicio ronda " + _currentRound);
    }

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
        _roundUI.SetActiveRanking(true);
        ShowRankingClientRpc();
    }

    [ClientRpc]
    private void ShowRankingClientRpc()
    {
        Debug.Log("NETCODE: Se muestra el ranking en todos");
        _roundUI.SetActiveRanking(true);
    }

    private void ShowFinalRanking()
    {
        Debug.Log("NETLESS: Se muestra el ranking final");
        _roundUI.SetActiveRankingFinal(true);
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
        _roundUI.SetActiveRanking(false);
        _roundUI.SetActivePowerUps(true);
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
}
