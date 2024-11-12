using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public List<(Vector3 pos, ulong? clientId)> _spawnPoints;

    void Awake()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            // Obtencion de los puntos de spawn

            Transform[] transforms = GetComponentsInChildren<Transform>();

            Debug.Log($"Spawn points encontrados: {transforms.Length - 1}");

            if (transforms.Length != 6)
            {
                _spawnPoints = new List<(Vector3 pos, ulong? clientId)>();

                for (int i = 1; i < transforms.Length; i++)
                {
                    _spawnPoints.Add((transforms[i].position, null));
                }

                /*for (int i = 0; i < _spawnPoints.Count; i++)
                {
                    Debug.Log($"Spawn point {i + 1}: ({_spawnPoints.ElementAt(i).tPosition.position.x}, {_spawnPoints.ElementAt(i).tPosition.position.y}. " +
                        $"Libre: {_spawnPoints.ElementAt(i).isFree})");
                }*/
            }

            //NetworkManager.Singleton.OnClientConnectedCallback += SetPlayerInSpawn;
            //NetworkManager.Singleton.OnClientDisconnectCallback += FreeSpawnPoint;
        }
    }

    public void SetPlayerInSpawn(ulong id)
    {
        for (int i = 0; i < _spawnPoints.Count; i++)
        {
            if (_spawnPoints[i].clientId == null)
            {
                Debug.Log($"Cliente {id} se coloca en spawn {i}");
                // En el primer spawn vacio almacena el id del jugador que ahora lo ocupara
                var newTupla = (_spawnPoints[i].pos, id);
                _spawnPoints[i] = newTupla;

                // Coloca al jugador de ID recibido en el punto de spawn
                NetworkManager.Singleton.ConnectedClients[id].PlayerObject.GetComponent<Transform>().position = _spawnPoints[i].pos;
                break;
            }
        }
    }

    public void FreeSpawnPoint(ulong id)
    {
        int index = -1;

        for (int i = 0; i < _spawnPoints.Count; i++)
        {
            if (_spawnPoints[i].clientId == id)
            {
                index = i; 
                break;
            }
        }

        if (index == -1)
        {
            Debug.LogWarning("Player not found in spawn points list");
            return;
        }

        // Se desplazan los ids de los clientes sobre el spawn point que ha quedado libre
        for (int i = index + 1; i < _spawnPoints.Count; i++)
        {
            _spawnPoints[i - 1] = (_spawnPoints[i - 1].pos, _spawnPoints[i].clientId);
        }

        // Se vacia el ultimo spawn point
        _spawnPoints[_spawnPoints.Count - 1] = (_spawnPoints[_spawnPoints.Count - 1].pos, null);
    }

    public void ResetSpawnPoints()
    {
        Debug.Log("Reseting all players position to spawn point");

        for (int i = 0; i < _spawnPoints.Count; i++)
        {
            if (_spawnPoints[i].clientId != null)
            {
                NetworkManager.Singleton.ConnectedClients[(ulong)_spawnPoints[i].clientId].PlayerObject.GetComponent<Transform>().position = _spawnPoints[i].pos;
            }
        }
    }
}
