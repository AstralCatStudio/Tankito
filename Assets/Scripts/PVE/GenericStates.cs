using System.Collections;
using System.Collections.Generic;
using Tankito.Netcode;
using UnityEngine;

public class GenericStates : MonoBehaviour
{
    [SerializeField] GameObject player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    public void IdleStartState()
    {

    }

    public Vector2 IdleState()
    {

        return Vector2.zero;
    }

    public Vector2 AimState()
    {
        Vector2 aimVec = player.transform.position - transform.position;
        CheckAimToShoot(aimVec);
        return Vector2.zero;
    }

    public bool CheckAimToShoot(Vector2 aimVec)
    {
        //if(aimVec == transform.GetChild(1))
        return false;
    }

    public TankAction ShootState()
    {
        return TankAction.Fire;
    }
}
