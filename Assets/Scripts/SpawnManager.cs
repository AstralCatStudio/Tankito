using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tankito;
using Unity.Netcode;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public List<(Transform transform, ulong? clientId)> m_spawnPoints;

    void Awake()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            // Obtencion de los puntos de spawn

            Transform[] transforms = GetComponentsInChildren<Transform>();

            Debug.Log($"Spawn points encontrados: {transforms.Length - 1}");

            if ((transforms.Length > 0) && transforms.Length <= 5)
            {
                m_spawnPoints = new List<(Transform transform, ulong? clientId)>();

                for (int i = 1; i < transforms.Length; i++)
                {
                    m_spawnPoints.Add((transforms[i], null));
                }

                for (int i = 0; i < m_spawnPoints.Count; i++)
                {
                    Debug.Log($"Spawn point {i + 1}: ({m_spawnPoints.ElementAt(i).transform.position.x}, {m_spawnPoints.ElementAt(i).transform.position.y}. " +
                        $"Libre: {m_spawnPoints.ElementAt(i).clientId == null})");
                }
            }
        }
        else
        {
            Destroy(this);
        }
    }

    public void SetPlayerInSpawn(ulong id)
    {
        for (int i = 0; i < m_spawnPoints.Count; i++)
        {
            if (m_spawnPoints[i].clientId == null)
            {
                Debug.Log($"Cliente {id} se coloca en spawn {i}");
                // En el primer spawn vacio almacena el id del jugador que ahora lo ocupara
                var newTupla = (m_spawnPoints[i].transform, id);
                m_spawnPoints[i] = newTupla;

                // Coloca al jugador de ID recibido en el punto de spawn
                GameObject player = NetworkManager.Singleton.ConnectedClients[id].PlayerObject.gameObject;
                GameManager.Instance.SetObjectPosition(player, m_spawnPoints[i].transform.position, m_spawnPoints[i].transform.rotation);
                break;
            }
            else if (m_spawnPoints[i].clientId != null)
            {
                Debug.Log($"Cliente {m_spawnPoints[i].clientId}, ya conectado, se coloca en spawn {i}");

                // Coloca al jugador ya conectado en el punto de spawn
                GameObject player = NetworkManager.Singleton.ConnectedClients[(ulong)m_spawnPoints[i].clientId].PlayerObject.gameObject;
                GameManager.Instance.SetObjectPositionClientRpc(player, m_spawnPoints[i].transform.position, m_spawnPoints[i].transform.rotation, id);
            }
        }
    }

    public void FreeSpawnPoint(ulong id)
    {
        int index = -1;

        for (int i = 0; i < m_spawnPoints.Count; i++)
        {
            if (m_spawnPoints[i].clientId == id)
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
        for (int i = index + 1; i < m_spawnPoints.Count; i++)
        {
            m_spawnPoints[i - 1] = (m_spawnPoints[i - 1].transform, m_spawnPoints[i].clientId);
        }

        // Se vacia el ultimo spawn point
        m_spawnPoints[m_spawnPoints.Count - 1] = (m_spawnPoints[m_spawnPoints.Count - 1].transform, null);

        // Si no se ha empezado la partida, se recolocan todos los tanques que han cambiado de spawn
        if (!FindObjectOfType<RoundManager>().IsGameStarted)
        {
            for (int i = index; i < m_spawnPoints.Count; i++)
            {
                if (m_spawnPoints[i].clientId != null)
                {
                    GameObject player = NetworkManager.Singleton.ConnectedClients[(ulong)m_spawnPoints[i].clientId].PlayerObject.gameObject;
                    GameManager.Instance.SetObjectPosition(player, m_spawnPoints[i].transform.position, m_spawnPoints[i].transform.rotation);
                }
            }
        }
    }

    public void ResetSpawnPoints()
    {
        Debug.Log("Reseting all players position to spawn point");

        for (int i = 0; i < m_spawnPoints.Count; i++)
        {
            if (m_spawnPoints[i].clientId != null)
            {
                Debug.Log($"Cliente {m_spawnPoints[i].clientId} recolocado en el spawn {i}");
                NetworkManager.Singleton.ConnectedClients[(ulong)m_spawnPoints[i].clientId].PlayerObject.GetComponent<Transform>().position = m_spawnPoints[i].transform.position;
                NetworkManager.Singleton.ConnectedClients[(ulong)m_spawnPoints[i].clientId].PlayerObject.GetComponent<Transform>().rotation = m_spawnPoints[i].transform.rotation;

                GameObject player = NetworkManager.Singleton.ConnectedClients[(ulong)m_spawnPoints[i].clientId].PlayerObject.gameObject;
                GameManager.Instance.SetObjectPosition(player, m_spawnPoints[i].transform.position, m_spawnPoints[i].transform.rotation);
            }
        }
    }

    private void ReleaseAllSpawnPoints()
    {
        Debug.Log("Vaciando todos los spawns");

        for (int i = 0; i < m_spawnPoints.Count; i++)
        {
            m_spawnPoints[i] = (m_spawnPoints[i].transform, null);
        }
    }

    public void RecalculateSpawnPoints()
    {
        Debug.Log("Has cambiado de escenario, recalculando spawn points...");

        ReleaseAllSpawnPoints();

        foreach (var client in NetworkManager.Singleton.ConnectedClients.Values)
        {
            Debug.Log($"Recolocando al cliente {client.ClientId}");
            for (int i = 0; i < m_spawnPoints.Count; i++)
            {
                if (m_spawnPoints[i].clientId == null)
                {
                    Debug.Log($"Cliente {client.ClientId} se coloca en spawn {i}");

                    var newTupla = (m_spawnPoints[i].transform, client.ClientId);
                    m_spawnPoints[i] = newTupla;

                    GameObject player = client.PlayerObject.gameObject;
                    GameManager.Instance.SetObjectPosition(player, m_spawnPoints[i].transform.position, m_spawnPoints[i].transform.rotation);
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
