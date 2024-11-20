using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Unity.Netcode.Transports.UTP;

public class LocalServerInitializer : MonoBehaviour
{
    void Start()
    {
        // Change Transport Type to UnityTransport instead of UnityRelayTransport
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(
            "127.0.0.1",  // The IP address is a string
            (ushort)12345, // The port number is an unsigned short
            "0.0.0.0" // The server listen address is a string.
        );

        // Spin up Local Host
        NetworkManager.Singleton.StartHost();
    }
}
