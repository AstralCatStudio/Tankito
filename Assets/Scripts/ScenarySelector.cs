using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class ScenarySelector : NetworkBehaviour
{
    private NetworkVariable<int> _currentScenaryIndex = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private List<GameObject> _scenaries = new List<GameObject>();

    void Start()
    {
        // Activar la ui del escenario en el server
        if (IsServer)
        {
            RoundUI roundUI = GameObject.FindObjectOfType<RoundUI>();
            if (roundUI != null)
            {
                roundUI.SetActiveScenaryButtons(true);
            }
        }

        // Obtener los hijos y añadirlos a la lista
        foreach (Transform child in transform)
        {
            _scenaries.Add(child.gameObject);
            if (child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(false);
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
        if (_scenaries.Count > 0 && _currentScenaryIndex.Value >= 0 && _currentScenaryIndex.Value < _scenaries.Count)
        {
            for(int i = 0; i < _scenaries.Count; i++)
            {
                _scenaries[i].SetActive(i == _currentScenaryIndex.Value);
            }
        }
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
        }

        if (IsServer)
        {
            // Volver a asignar spawns
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
}
