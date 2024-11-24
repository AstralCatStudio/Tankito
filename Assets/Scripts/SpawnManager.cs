using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tankito;
using Unity.Netcode;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public List<(Transform transform, ulong? clientId)> _spawnPoints;

    void Awake()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            // Obtencion de los puntos de spawn

            Transform[] transforms = GetComponentsInChildren<Transform>();

            Debug.Log($"Spawn points encontrados: {transforms.Length - 1}");

            if ((transforms.Length > 0) && transforms.Length <= 5)
            {
                _spawnPoints = new List<(Transform transform, ulong? clientId)>();

                for (int i = 1; i < transforms.Length; i++)
                {
                    _spawnPoints.Add((transforms[i], null));
                }

                for (int i = 0; i < _spawnPoints.Count; i++)
                {
                    Debug.Log($"Spawn point {i + 1}: ({_spawnPoints.ElementAt(i).transform.position.x}, {_spawnPoints.ElementAt(i).transform.position.y}. " +
                        $"Libre: {_spawnPoints.ElementAt(i).clientId == null})");
                }
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
                var newTupla = (_spawnPoints[i].transform, id);
                _spawnPoints[i] = newTupla;

                // Coloca al jugador de ID recibido en el punto de spawn
                GameObject player = NetworkManager.Singleton.ConnectedClients[id].PlayerObject.gameObject;
                GameManager.Instance.SetObjectPosition(player, _spawnPoints[i].transform.position, _spawnPoints[i].transform.rotation);
                break;
            }
            else if (_spawnPoints[i].clientId != null)
            {
                Debug.Log($"Cliente {_spawnPoints[i].clientId}, ya conectado, se coloca en spawn {i}");

                // Coloca al jugador ya conectado en el punto de spawn
                GameObject player = NetworkManager.Singleton.ConnectedClients[(ulong)_spawnPoints[i].clientId].PlayerObject.gameObject;
                GameManager.Instance.SetObjectPositionClientRpc(player, _spawnPoints[i].transform.position, _spawnPoints[i].transform.rotation, id);
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
            _spawnPoints[i - 1] = (_spawnPoints[i - 1].transform, _spawnPoints[i].clientId);
        }

        // Se vacia el ultimo spawn point
        _spawnPoints[_spawnPoints.Count - 1] = (_spawnPoints[_spawnPoints.Count - 1].transform, null);

        // Si no se ha empezado la partida, se recolocan todos los tanques que han cambiado de spawn
        if (!FindObjectOfType<RoundManager>().IsGameStarted)
        {
            for (int i = index; i < _spawnPoints.Count; i++)
            {
                if (_spawnPoints[i].clientId != null)
                {
                    GameObject player = NetworkManager.Singleton.ConnectedClients[(ulong)_spawnPoints[i].clientId].PlayerObject.gameObject;
                    GameManager.Instance.SetObjectPosition(player, _spawnPoints[i].transform.position, _spawnPoints[i].transform.rotation);
                }
            }
        }
    }

    public void ResetSpawnPoints()
    {
        Debug.Log("Reseting all players position to spawn point");

        for (int i = 0; i < _spawnPoints.Count; i++)
        {
            if (_spawnPoints[i].clientId != null)
            {
                Debug.Log($"Cliente {_spawnPoints[i].clientId} recolocado en el spawn {i}");
                NetworkManager.Singleton.ConnectedClients[(ulong)_spawnPoints[i].clientId].PlayerObject.GetComponent<Transform>().position = _spawnPoints[i].transform.position;
                NetworkManager.Singleton.ConnectedClients[(ulong)_spawnPoints[i].clientId].PlayerObject.GetComponent<Transform>().rotation = _spawnPoints[i].transform.rotation;

                GameObject player = NetworkManager.Singleton.ConnectedClients[(ulong)_spawnPoints[i].clientId].PlayerObject.gameObject;
                GameManager.Instance.SetObjectPosition(player, _spawnPoints[i].transform.position, _spawnPoints[i].transform.rotation);
            }
        }
    }

    private void ReleaseAllSpawnPoints()
    {
        Debug.Log("Vaciando todos los spawns");

        for (int i = 0; i < _spawnPoints.Count; i++)
        {
            _spawnPoints[i] = (_spawnPoints[i].transform, null);
        }
    }

    public void RecalculateSpawnPoints()
    {
        Debug.Log("Has cambiado de escenario, recalculando spawn points...");

        ReleaseAllSpawnPoints();

        foreach (var client in NetworkManager.Singleton.ConnectedClients.Values)
        {
            Debug.Log($"Recolocando al cliente {client.ClientId}");
            for (int i = 0; i < _spawnPoints.Count; i++)
            {
                if (_spawnPoints[i].clientId == null)
                {
                    Debug.Log($"Cliente {client.ClientId} se coloca en spawn {i}");

                    var newTupla = (_spawnPoints[i].transform, client.ClientId);
                    _spawnPoints[i] = newTupla;

                    GameObject player = client.PlayerObject.gameObject;
                    GameManager.Instance.SetObjectPosition(player, _spawnPoints[i].transform.position, _spawnPoints[i].transform.rotation);
                    break;
                }
                else
                {
                    Debug.Log($"El spawn {i} esta ocupado");
                }
            }
        }
    }
}
