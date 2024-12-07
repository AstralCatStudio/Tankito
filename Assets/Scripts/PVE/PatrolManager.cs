using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PatrolManager : Singleton<PatrolManager>
{
    [SerializeField] List<Transform> patrolPoints = new List<Transform>();
    GameObject player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    public Transform GetPatrolPoint()
    {
        int n = Random.Range(0, patrolPoints.Count);
        return patrolPoints[n];
    }

    public Transform GetDigAppearPoint(int maxDigDistance)
    {
        List<Transform> possiblePoints = patrolPoints.Where(obj => Vector2.Distance(obj.position, player.transform.position) <= maxDigDistance).ToList();
        int n = Random.Range(0, possiblePoints.Count);
        return possiblePoints[n];

    }
}
