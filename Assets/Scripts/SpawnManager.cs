using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public List<(Vector3 pos, bool isFree)> _spawnPoints;

    void Awake()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            Transform[] transforms = GetComponentsInChildren<Transform>();

            Debug.Log($"Spawn points encontrados: {transforms.Length - 1}");

            if (transforms.Length != 6)
            {
                _spawnPoints = new List<(Vector3 pos, bool isFree)>();

                for (int i = 1; i < transforms.Length; i++)
                {
                    _spawnPoints.Add((transforms[i].position, true));
                }

                /*for (int i = 0; i < _spawnPoints.Count; i++)
                {
                    Debug.Log($"Spawn point {i + 1}: ({_spawnPoints.ElementAt(i).tPosition.position.x}, {_spawnPoints.ElementAt(i).tPosition.position.y}. " +
                        $"Libre: {_spawnPoints.ElementAt(i).isFree})");
                }*/
            }
        }
    }

    public Vector3 GetSpawnPoint()
    {
        for (int i = 0; i < _spawnPoints.Count; i++)
        {
            if (_spawnPoints[i].isFree)
            {
                var newTupla = (_spawnPoints[i].pos, false);
                _spawnPoints[i] = newTupla;
                return _spawnPoints[i].pos;
            }
        }

        return Vector3.zero;
    }

    public void FreeSpawnPoint(Vector3 spawnPoint)
    {
        for (int i = 0; i < _spawnPoints.Count; i++)
        {
            if (_spawnPoints[i].pos == spawnPoint)
            {
                var newTupla = (_spawnPoints[i].pos, true);
                _spawnPoints[i] = newTupla;
                break;
            }
        }
    }

    public void ResetSpawnPoints()
    {
        for (int i = 0; i < _spawnPoints.Count; i++)
        {
            if (!_spawnPoints[i].isFree)
            {
                var newTupla = (_spawnPoints[i].pos, true);
                _spawnPoints[i] = newTupla;
            }
        }
    }
}
