using System.Collections;
using System.Collections.Generic;
using Tankito;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DisconnectHandler : NetworkBehaviour
{
    public static DisconnectHandler Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ExitGame()
    {
        if (IsServer)
        {
            DespawnAllClients();
            
            DisconnectAllClientRpc();

            Invoke(nameof(Disconnect), 0.7f);
        }
        else
        {
            DespawnClientServerRpc(NetworkManager.Singleton.LocalClientId);

            Disconnect();
        }
    }

    private void DespawnAllClients()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject != null && client.PlayerObject.TryGetComponent<NetworkObject>(out var networkObject))
            {
                networkObject.Despawn();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DespawnClientServerRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            var playerObject = client.PlayerObject;

            if (playerObject != null)
            {
                playerObject.Despawn(true);
                Debug.Log($"Jugador con ClientID {clientId} ha sido despaweado.");
            }
            else
            {
                Debug.LogWarning($"No se encontró un PlayerObject para el ClientID {clientId}.");
            }
        }
        else
        {
            Debug.LogWarning($"El ClientID {clientId} no está conectado.");
        }
    }

    private void Disconnect()
    {
        SimClock.Instance.StopClock();

        NetworkManager.Singleton.Shutdown();

        SceneLoader.Singleton.ReloadMainMenu();
    }

    [ClientRpc]
    private void DisconnectAllClientRpc()
    {
        if (!IsServer)
        {
            Disconnect();
        }
    }
}
