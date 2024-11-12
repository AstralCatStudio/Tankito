using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class TankData : NetworkBehaviour
{
    public delegate void TankDestroyedHandler(GameObject tank);
    public static event TankDestroyedHandler OnTankDestroyed;

    public NetworkVariable<int> health = new NetworkVariable<int>(2);
    public NetworkVariable<bool> isAlive = new NetworkVariable<bool>(true);

    private Vector3 _spawnPoint;

    public int points;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            RegisterToRoundManager();
            SetInSpawnPoint();
            points = 0;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (IsServer)
        {
            //RegisterToRoundManager();
            RoundManager roundManager = FindObjectOfType<RoundManager>();
            if (!roundManager._startedGame)
            {
                FreeSpawnPoint();
            }
        }
    }

    private void RegisterToRoundManager()
    {
        RoundManager roundManager = FindObjectOfType<RoundManager>();

        if (roundManager != null)
        {
            roundManager.AddPlayer(gameObject);
        }
    }

    public void SetInSpawnPoint()
    {
        SpawnManager spawnManager = FindObjectOfType<SpawnManager>();
        _spawnPoint = spawnManager.GetSpawnPoint();
        GetComponent<Transform>().position = _spawnPoint;
    }

    public void FreeSpawnPoint()
    {
        SpawnManager spawnManager = FindObjectOfType<SpawnManager>();
        spawnManager.FreeSpawnPoint(_spawnPoint);

        //GetComponent<Transform>().position = spawnManager.FreeSpawnPoint();
    }

    public void Die()
    {
        if (IsServer)
        {
            OnTankDestroyed?.Invoke(gameObject);
            Debug.Log($"{gameObject.name} ha sido destruido");
        }
    }

    public void TakeDamage(int damage)
    {
        if (IsServer)
        {
            health.Value -= damage;
            if (health.Value <= 0)
            {
                isAlive.Value = false;
                Die();
            }
        }
    }
    public void ResetTank()
    {
        health.Value = 2;
        isAlive.Value = true;
    }

    public void IncreasePoints(int newPoints)
    {
        points += newPoints;
    }
}
