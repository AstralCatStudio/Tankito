using BehaviourAPI.Core;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tankito.SinglePlayer;
using UnityEngine;
using static AreaDetection;

public class KamikazeBehaviour : AGenericBehaviour
{
    [SerializeField] private float explosionRange = 2.0f;

    private GameObject targetPlayer;

    private bool hasBlocked = false;
    private bool hasCollided = false;

    protected override void Start()
    {
        base.Start();
    }

    #region States
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    

    public Status ParryState()
    {
        Debug.Log("PARRY KAMIKAZE");
        return Status.Success;
    }

    public Status ExplodeState()
    {
        Debug.Log("KAMIKAZE EXPLOTA");
        Destroy(gameObject);
        return Status.Success;
    }
    #endregion

    #region Perceptions

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public bool IsPlayerReached()
    {
        if (targetPlayer != null)
        {
            float distance = Vector2.Distance(transform.position, targetPlayer.transform.position);
            return distance <= explosionRange;
        }
        return false;
    }

    public bool BulletDetected()
    {
        return false;
    }

    public bool HasBlocked()
    {
        return hasBlocked;
    }

    public bool NoHasBlocked()
    {
        return !hasBlocked;
    }

    public bool HasCollided() // con player (por si a caso) o bala
    {
        return hasCollided;
    }

    #endregion

    #region Utilities
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Bullet"))
        {
            hasCollided = true;
            Debug.Log("COLLISION DE KAMIKAZE");
        }
    }
    #endregion
}
