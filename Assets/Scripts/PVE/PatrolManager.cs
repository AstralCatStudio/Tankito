using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PatrolManager : Singleton<PatrolManager>
{
    [SerializeField] List<Transform> patrolPoints = new List<Transform>();
    [SerializeField] float patrolDistanceMax= 15f;
    GameObject player;

    protected override void Awake()
    {
        base.Awake();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    public Transform GetPatrolPoint()
    {
        List<Transform> possiblePoints;
        if (player != null)
        {
            possiblePoints = patrolPoints.Where(obj => Vector2.Distance(obj.position, player.transform.position) <= patrolDistanceMax).ToList();
        }
        possiblePoints = patrolPoints;
        int n = Random.Range(0, possiblePoints.Count);
        return possiblePoints[n];
    }

    public Transform GetDigAppearPoint(int maxDigDistance)
    {
        List<Transform> possiblePoints = patrolPoints.Where(obj => Vector2.Distance(obj.position, player.transform.position) <= maxDigDistance).ToList();
        int n = Random.Range(0, possiblePoints.Count);
        return possiblePoints[n];

    }
}
