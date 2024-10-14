using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    private NetworkManager _networkManager;
    private GameObject _playerPrefab;

    private string _playerName = "Invited";

    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        _networkManager = NetworkManager.Singleton;
        _playerPrefab = _networkManager.NetworkConfig.Prefabs.Prefabs[0].Prefab;

        _networkManager.OnServerStarted += OnServerStarted;
        _networkManager.OnClientConnectedCallback += OnClientConnected;

        //_playerName = "Invited";
    }

    private void OnServerStarted()
    {
        print("Servidor inicalizado.");
    }

    private void OnClientConnected(ulong obj)
    {
        if (IsServer)
        {
            print("Cliente se conecta y spawnea el jugador");
            var newPlayer = Instantiate(_playerPrefab);
            newPlayer.GetComponent<NetworkObject>().SpawnAsPlayerObject(obj);
        }
    }

    public void SetPlayerName(string name)
    {
        _playerName = name;
        Debug.Log("GameManager guarda el nombre:" + _playerName);
    }

    public string GetPlayerName()
    {
        return _playerName;
    }
}