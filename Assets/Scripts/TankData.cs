using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using Tankito;

public class TankData : NetworkBehaviour
{
    public delegate void TankDestroyedHandler(GameObject tank);
    public static event TankDestroyedHandler OnTankDestroyed;
    public Action<TankData> OnDamaged = (TankData) => { };
    public NetworkVariable<int> health = new NetworkVariable<int>(2);
    public NetworkVariable<bool> isAlive = new NetworkVariable<bool>(true);
    public int points;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        points = 0;
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
        OnDamaged(this);
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
        Debug.Log("Reseting tank");
        health.Value = 2;
        isAlive.Value = true;
    }

    public void IncreasePoints(int newPoints)
    {
        points += newPoints;
    }
}
