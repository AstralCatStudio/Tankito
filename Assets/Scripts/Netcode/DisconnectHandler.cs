using System.Collections;
using System.Collections.Generic;
using Tankito;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Tankito.Netcode.Messaging;

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
            DisconnectAllClientRpc();

            Invoke(nameof(Disconnect), 0.7f);
        }
        else
        {
            Disconnect();
        }
    }

    private void Disconnect()
    {
        SimClock.Instance.StopClock();

        UnregisterMessageHandler();

        NetworkManager.Singleton.Shutdown();

        SceneLoader.Singleton.ReloadMainMenu();
    }

    private void UnregisterMessageHandler()
    {
        //Debug.Log("Unregistering in Message Handler");

        if (NetworkManager.Singleton.CustomMessagingManager != null)
        {
            // De-register when the associated NetworkObject is despawned.
            NetworkManager.Singleton.CustomMessagingManager.UnregisterNamedMessageHandler(MessageName.ClockSignal);
            NetworkManager.Singleton.CustomMessagingManager.UnregisterNamedMessageHandler(MessageName.InputWindow);
            NetworkManager.Singleton.CustomMessagingManager.UnregisterNamedMessageHandler(MessageName.RelayInputWindow);
            NetworkManager.Singleton.CustomMessagingManager.UnregisterNamedMessageHandler(MessageName.SimulationSnapshot);
        }
        else
        {
            Debug.LogWarning("Must have been disconnected, skipping message unregistering.");
        }
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
