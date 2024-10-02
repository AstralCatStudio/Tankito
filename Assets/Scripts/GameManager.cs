using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    private NetworkManager _networkManager;
    private GameObject _playerPrefab;

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

    }

    private void OnServerStarted()
    {
        print("Servidor inicalizado.");
    }

    private void OnClientConnected(ulong obj)
    {
        if (IsServer)
        {
            print("Cliente se conecta");
            var newPlayer = Instantiate(_playerPrefab);
            newPlayer.GetComponent<NetworkObject>().SpawnAsPlayerObject(obj);
        }
    }
}
