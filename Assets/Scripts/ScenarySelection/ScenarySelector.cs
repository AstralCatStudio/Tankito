using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEditor.Rendering.LookDev;

namespace Tankito.ScenarySelection
{
    public class ScenarySelector : NetworkBehaviour
    {
        private NetworkVariable<int> _currentScenaryIndex = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private NetworkVariable<int> _currentMapIndex = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private List<GameObject> _scenaries = new List<GameObject>();

        [SerializeField] private int MapsPerScenary;

        #region NetworkBehaviour

        private void Awake()
        {
            // Obtener los hijos (escenarios) y añadirlos a la lista
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
                // RoundUI roundUI = GameObject.FindObjectOfType<RoundUI>();
                if (RoundUI.Instance != null)
                {
                    RoundUI.Instance.SetActiveScenarySelection(true);
                    RoundUI.Instance.SetScenaryText(_scenaries[_currentScenaryIndex.Value].name);
                }
                else
                {
                    Debug.LogWarning("ScenarySelector: RoundUI no se encontró");
                }
            }

            // Suscribirse al cambio de la network variable
            _currentScenaryIndex.OnValueChanged += OnScenaryIndexChanged;
        }

        private void Update()
        {
            //Debug.LogWarning($"Mapa de la network variable {_currentMapIndex.Value}");
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

            ActivateScenary((int)_currentScenaryIndex.Value, true);
            ActivateMap((int)_currentScenaryIndex.Value, (int)_currentMapIndex.Value, true);
            UpdateScenaryMusic();
        }

        #endregion

        #region NetworkVariables

        private void OnScenaryIndexChanged(int oldIndex, int newIndex)
        {
            Debug.Log($"OldIndex: {oldIndex}, NewIndex: {newIndex}");
            if (oldIndex >= 0 && oldIndex < _scenaries.Count)
            {
                ActivateScenary(oldIndex, false);
            }

            if (newIndex >= 0 && newIndex < _scenaries.Count)
            {
                ActivateScenary(newIndex, true);
                UpdateScenaryText(_scenaries[newIndex].name);
            }

            SetRandomMap();

            UpdateScenaryMusic();
        }

        #endregion



        #region ScenaryChange

        private void ActivateScenary(int index, bool active)
        {
            if (active)
            {
                for (int i = 0; i < _scenaries.Count; i++)
                {
                    _scenaries[i].SetActive(false);
                }
            }

            _scenaries[index].SetActive(active);

            if (active)
            {
                Debug.Log($"Activado escenario de index {index}: {_scenaries[index].name}");
            }
            else
            {
                Debug.Log($"Desactivado escenario de index {index}: {_scenaries[index].name}");
            }
        }

        private void ActivateMap(int scenaryIndex, int mapIndex, bool active)
        {
            for (int i = 0; i < MapsPerScenary; i++)
            {
                _scenaries[scenaryIndex].transform.GetChild(i).gameObject.SetActive(false);
                //Debug.Log($"Mapa {i} del escenario {scenaryIndex} desactivado");
            }
            _scenaries[scenaryIndex].transform.GetChild(mapIndex).gameObject.SetActive(true);
            Debug.Log($"Mapa {mapIndex} del escenario {scenaryIndex} activado");
            //Debug.LogWarning("Se activa el mapa " + mapIndex + " de nombre "+ _scenaries[scenaryIndex].transform.GetChild(mapIndex).gameObject.name);
        }

        [ClientRpc]
        private void ActivateCurrentMapClientRpc(int currentScenary, int currentMap)
        {
            Debug.LogWarning("Recibido del server" + currentMap);
            ActivateMap(currentScenary, currentMap, true);
        }

        public void SetRandomMap()
        {
            if (!IsServer) return;

            _currentMapIndex.Value = Random.Range(0, MapsPerScenary);
            Debug.LogWarning($"Mapa aleatorio: {_currentMapIndex.Value}");
            ActivateCurrentMapClientRpc(_currentScenaryIndex.Value, _currentMapIndex.Value);

            RecalculateSpawnPoints();
        }

        private void RecalculateSpawnPoints()
        {
            if (!IsServer) return;

            // Volver a asignar spawns
            SpawnManager spawnManager = _scenaries[_currentScenaryIndex.Value].GetComponentInChildren<SpawnManager>();
            if (spawnManager != null)
            {
                spawnManager.RecalculateSpawnPoints();
            }
            else
            {
                Debug.LogWarning("ScenarySelector: No se encontro spawnManager");
            }
        }

        #endregion



        #region Buttons

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

        #endregion



        #region UI
        private void UpdateScenaryText(string text)
        {
            if (IsServer)
            {
                //RoundUI roundUI = GameObject.FindObjectOfType<RoundUI>();
                if (RoundUI.Instance != null)
                {
                    RoundUI.Instance.SetScenaryText(text);
                }
                else
                {
                    Debug.LogWarning("ScenarySelector: RoundUI no se encontró");
                }
            }
        }

        #endregion



        #region Music

        public int GetActiveBiome()
        {
            Debug.Log($"Escenario activo: {_currentScenaryIndex.Value}");
            return _currentScenaryIndex.Value;
        }

        private void UpdateScenaryMusic()
        {
            // Background sound
            switch (GetActiveBiome())
            {
                case 0:
                    MusicManager.Instance.PlayBackgroundSound("amb_beach");
                    MusicManager.Instance.SetReverbZone(1, 5f, 20f);
                    break;

                case 1:
                    MusicManager.Instance.PlayBackgroundSound("amb_sushi");
                    MusicManager.Instance.SetReverbZone(4, 5f, 20f);
                    break;

                case 2:
                    MusicManager.Instance.PlayBackgroundSound("amb_barco");
                    MusicManager.Instance.SetReverbZone(0, 5f, 20f);
                    break;

                default:
                    MusicManager.Instance.PlayBackgroundSound("amb_beach");
                    MusicManager.Instance.SetReverbZone(1, 5f, 20f);
                    break;
            }
            // sound effect
            MusicManager.Instance.PlaySound("bip");
        }
        #endregion
    }
}