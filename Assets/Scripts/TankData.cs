using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using Tankito;

public class TankData : NetworkBehaviour
{
    public delegate void TankDestroyedHandler(TankData tank);
    public static event TankDestroyedHandler OnTankDestroyed = (TankData tank) => { };
    public Action<TankData> OnDamaged = (TankData damagedTank) => { };
    private NetworkVariable<int> m_health = new NetworkVariable<int>(2);
    private NetworkVariable<bool> m_isAlive = new NetworkVariable<bool>(true);
    private int m_points;
    public int Health => m_health.Value;
    public bool Alive => m_isAlive.Value;
    public int Points => m_points;

    void Start()
    {
        m_points = 0;
    }

    public void Die()
    {
        if (IsServer)
        {
            DieClientRpc();
        }
        OnTankDestroyed?.Invoke(this);
        Debug.LogWarning("TODO: Trigger tank death animation");
        gameObject.SetActive(false);
    }

    [ClientRpc]
    public void DieClientRpc()
    {
        if (!IsServer)
        {
            Die();
        }
    }

    public void AwardPoints(int awardedPoints)
    {
        m_points += awardedPoints;
    }

    public void TakeDamage(int damage)
    {
        OnDamaged(this);
        if (IsServer)
        {
            m_health.Value -= damage;
            
            if (m_health.Value <= 0)
            {
                m_isAlive.Value = false;
                Die();
            }
        }
    }

    public void AddHealth(int addedHealth)
    {
        m_health.Value += addedHealth;
    }
    public void SetHealth(int newHealth)
    {
        m_health.Value = newHealth;
    }

    public void ResetTank()
    {
        //Debug.LogWarning("TODO: maybe play spawn animation?");
        gameObject.SetActive(true);
        m_isAlive.Value = true;
    }
}
