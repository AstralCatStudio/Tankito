using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class ScenarySelector : NetworkBehaviour
{
    private NetworkVariable<int> _currentScenaryIndex = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private List<GameObject> _scenaries = new List<GameObject>();

    private void Awake()
    {
        // Obtener los hijos y añadirlos a la lista
        foreach (Transform child in transform)
        {
            _scenaries.Add(child.gameObject);
            Debug.Log($"Anadido escenario {child.gameObject.name}");
        }
        Debug.Log($"Escenarios encontrados: {_scenaries.Count}");
    }

    void Start()
    {
        // Activar la ui del escenario en el server
        if (IsServer)
        {
            RoundUI roundUI = GameObject.FindObjectOfType<RoundUI>();
            if (roundUI != null)
            {
                roundUI.SetActiveScenarySelection(true);
                roundUI.SetScenaryText(_scenaries[_currentScenaryIndex.Value].name);
            }
            else
            {
                Debug.LogWarning("ScenarySelector: RoundUI no se encontró");
            }
        }

        // Suscribirse al cambio de la network variable
        _currentScenaryIndex.OnValueChanged += OnScenaryIndexChanged;
    }
    
    public override void OnDestroy()
    {
        base.OnDestroy();

        // Desuscribirse al cambio de la network variable
        _currentScenaryIndex.OnValueChanged -= OnScenaryIndexChanged;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Activar el escenario actual al spawnear
        Debug.Log($"Spawneo y activo el escenario {_currentScenaryIndex.Value}");
        ActivateScenary((int)_currentScenaryIndex.Value);
    }

    private void OnScenaryIndexChanged(int oldIndex, int newIndex)
    {
        Debug.Log($"OldIndex: {oldIndex}, NewIndex: {newIndex}");
        if (oldIndex >= 0 && oldIndex < _scenaries.Count)
        {
            _scenaries[oldIndex].SetActive(false);
        }

        if (newIndex >= 0 && newIndex < _scenaries.Count)
        {
            _scenaries[newIndex].SetActive(true);
            UpdateScenaryText(_scenaries[newIndex].name);
        }

        if (IsServer)
        {
            // Volver a asignar spawns
            SpawnManager spawnManager = GameObject.FindObjectOfType<SpawnManager>();
            if(spawnManager != null)
            {
                spawnManager.RecalculateSpawnPoints();
            }
            else
            {
                Debug.LogWarning("ScenarySelector: No se encontro spawnManager");
            }
        }
    }

    public void NextScenary()
    {
        if (!IsServer) return;

        Debug.Log("Next scenary button pressed");

        int newIndex = (_currentScenaryIndex.Value + 1) % _scenaries.Count;
        _currentScenaryIndex.Value = newIndex;
    }

    public void PreviousScenary()
    {
        if (!IsServer) return;

        Debug.Log("Previous scenary button pressed");

        int newIndex = (_currentScenaryIndex.Value - 1 + _scenaries.Count) % _scenaries.Count;
        _currentScenaryIndex.Value = newIndex;
    }

    private void ActivateScenary(int index)
    {
        for(int i = 0; i < _scenaries.Count; i++)
        {
            _scenaries[i].SetActive(i == index);
        }
        Debug.Log($"Activado escenario de index {index}: {_scenaries[index].name}");
    }

    private void UpdateScenaryText(string text)
    {
        if(IsServer)
        {
            RoundUI roundUI = GameObject.FindObjectOfType<RoundUI>();
            if (roundUI != null)
            {
                roundUI.SetScenaryText(text);
            }
            else
            {
                Debug.LogWarning("ScenarySelector: RoundUI no se encontró");
            }
        }
    }

    public int GetActiveBiome()
    {
        Debug.Log($"ESCENARIO ACTIVO: {_currentScenaryIndex.Value / 2}");
        return _currentScenaryIndex.Value/2;
    }
}
