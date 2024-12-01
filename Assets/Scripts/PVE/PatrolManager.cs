using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolManager : Singleton<PatrolManager>
{
    List<Transform> patrolPoints = new List<Transform>();

    public Transform GetPatrolPoint()
    {
        int n = Random.Range(0, patrolPoints.Count);
        return patrolPoints[n];
    }
}
