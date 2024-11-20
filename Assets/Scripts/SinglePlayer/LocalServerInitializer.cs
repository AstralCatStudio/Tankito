using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Unity.Netcode.Transports.UTP;
using System;

namespace Tankito
{
    public class LocalServerInitializer : MonoBehaviour
    {
        void Awake()
        {
            var unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();

            unityTransport.UseEncryption = false;
            // Change Transport Type to UnityTransport instead of UnityRelayTransport
            unityTransport.SetConnectionData(
                "127.0.0.1",  // The IP address is a string
                (ushort)12345 // The port number is an unsigned short
            );

            // Spin up Local Host
            if (!NetworkManager.Singleton.StartHost())
            {
                throw new Exception("Failed to spin up local server");
            }
            else
            {
                Debug.Log("Fired up local server succesfully!");
            }

            GameManager.Instance.singlePlayerGame = true;

            SceneLoader.Singleton.LoadSinglePlayerGameScene();
        }
    }

}