using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public List<(Transform tPosition, bool isFree)> _spawnPoints;

    void Start()
    {
        Transform[] transforms = GetComponentsInChildren<Transform>();

        Debug.Log($"Spawn points encontrados: {transforms.Length - 1}");

        if (transforms.Length != 6)
        {
            Debug.Log("Spawns encontrados");

            _spawnPoints = new List<(Transform tPosition, bool isFree)>();

            for (int i = 1; i < transforms.Length; i++)
            {
                _spawnPoints.Add((transforms[i], true));
            }

            Debug.Log("Spawn points a�adidos a la lista");

            for (int i = 0; i < _spawnPoints.Count; i++)
            {
                Debug.Log($"Spawn point {i + 1}: ({_spawnPoints.ElementAt(i).tPosition.position.x}, {_spawnPoints.ElementAt(i).tPosition.position.y}. " +
                    $"Libre: {_spawnPoints.ElementAt(i).isFree})");
            }
        }

    }

    public Vector3 GetSpawnPoint()
    {
        for (int i = 0; i < _spawnPoints.Count; i++)
        {
            if (_spawnPoints[i].isFree)
            {
                var newTupla = (_spawnPoints[i].tPosition, false);
                _spawnPoints[i] = newTupla;
                return _spawnPoints[i].tPosition.position;
            }
        }

        return Vector3.zero;
    }

    public void ResetSpawnPoints()
    {
        for (int i = 0; i < _spawnPoints.Count; i++)
        {
            if (!_spawnPoints[i].isFree)
            {
                var newTupla = (_spawnPoints[i].tPosition, true);
                _spawnPoints[i] = newTupla;
            }
        }
    }
}
