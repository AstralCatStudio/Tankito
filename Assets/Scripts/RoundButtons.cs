using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class RoundButtons : MonoBehaviour
{
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _readyButton;

    //private NetworkVariable<int> _readyCount = new NetworkVariable<int>(0);

    private void Start()
    {
        if(NetworkManager.Singleton.IsServer)
        {
            _startButton.gameObject.SetActive(true);
            _startButton.onClick.AddListener(OnStartPressed);
        }
        else if(NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            _readyButton.gameObject.SetActive(true);
            _readyButton.onClick.AddListener(OnReadyPressed);
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            _startButton.onClick.RemoveListener(OnStartPressed);
        }
        else if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            _readyButton.onClick.RemoveListener(OnReadyPressed);
        }
    }



    private void OnStartPressed()
    {
        Debug.Log("Pulsaste Start");
    }
    
    private void OnReadyPressed()
    {
        Debug.Log("Pulsaste Ready");
        RoundManager rm = GameObject.Find("RoundManager").GetComponent<RoundManager>();
        if(rm != null)
        {
            Debug.Log("RM encontrado");
        }
    }
}
