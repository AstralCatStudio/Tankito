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
        NetworkManager.Singleton.Shutdown();
        GameManager.Instance.UnloadScene();

        SceneManager.LoadScene("MainMenu");
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
