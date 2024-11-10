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

    public int points;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            RegisterToRoundManager();
            points = 0;
        }
    }

    private void RegisterToRoundManager()
    {
        RoundManager roundManager = FindObjectOfType<RoundManager>();

        if(roundManager != null )
        {
            roundManager.AddPlayer(gameObject);
        }
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
            if(health.Value <= 0 )
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
