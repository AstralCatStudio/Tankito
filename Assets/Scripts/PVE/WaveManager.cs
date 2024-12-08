using System.Collections;
using System.Collections.Generic;
using Unity.Services.Matchmaker.Models;
using UnityEngine;

namespace Tankito.SinglePlayer
{
    public class WaveManager : Singleton<WaveManager>
    {
        [Header("Wave configurations")]
        [SerializeField] private List<WaveData> waveConfigs;

        [Header("Enemy prefabs")]
        [SerializeField] private GameObject bodyGuardPrefab;
        [SerializeField] private GameObject healerPrefab;
        [SerializeField] private GameObject kamikazePrefab;
        [SerializeField] private GameObject necromancerPrefab;
        [SerializeField] private GameObject attackerPrefab;
        [SerializeField] private GameObject minerPrefab;

        [Header("Spawn Points")]
        [SerializeField] private Transform spawnsParent;
        [SerializeField] private List<Transform> spawnPoints;

        [Header("Spawning variables")]
        [SerializeField] private float maxDistance;
        [SerializeField] private bool isSpawning;

        [Header("Timer")]
        [SerializeField] private float waveTimeOut;
        [SerializeField] private float waveTimer;

        [Header("Current State")]
        [SerializeField] private List<GameObject> activeEnemies;
        [SerializeField] private int currentWave;
        [SerializeField] private int currentWaveIndex = -1;
        [SerializeField] GameObject player;

        protected override void Awake()
        {
            base.Awake();
            activeEnemies = new List<GameObject>();
            spawnPoints = new List<Transform>();
            currentWave = 0;
            isSpawning = true;
            foreach (Transform t in spawnsParent)
            {
                spawnPoints.Add(t);
            }
        }

        private void Start()
        {
            SpawnWave();
        }

        private void Update()
        {
            if (!isSpawning)
            {
                waveTimer += Time.deltaTime;
                SinglePlayerUI.Instance.SetWaveTime((int)waveTimer, (int)waveTimeOut);

                if (waveTimer > waveTimeOut || activeEnemies.Count <= 0)
                {
                    SpawnWave();
                }
            }
        }

        private void SpawnWave()
        {
            player.GetComponent<PVECharacterData>().AddHealth(1);
            isSpawning = true;

            waveTimer = 0f;
            SinglePlayerUI.Instance.SetDefaultWaveTimer();

            currentWaveIndex = Random.Range(0, waveConfigs.Count);
            WaveData newWave = waveConfigs[currentWaveIndex];
            currentWave++;
            SinglePlayerUI.Instance.SetCurrentWave(currentWave);
            Debug.Log($"Spawneando la oleada {currentWave}");
            StartCoroutine(SpawnEnemiesWithDelay(newWave));
        }

        private IEnumerator SpawnEnemiesWithDelay(WaveData newWave)
        {
            yield return StartCoroutine(SpawnEnemyWithDelay(newWave.BodyGuards, bodyGuardPrefab));
            yield return StartCoroutine(SpawnEnemyWithDelay(newWave.Healers, healerPrefab));
            yield return StartCoroutine(SpawnEnemyWithDelay(newWave.Kamikazes, kamikazePrefab));
            yield return StartCoroutine(SpawnEnemyWithDelay(newWave.Necromancers, necromancerPrefab));
            yield return StartCoroutine(SpawnEnemyWithDelay(newWave.Attackers, attackerPrefab));
            yield return StartCoroutine(SpawnEnemyWithDelay(newWave.Miners, minerPrefab));

            isSpawning = false;
        }

        private IEnumerator SpawnEnemyWithDelay(int count, GameObject enemy)
        {
            Debug.Log($"Spawning {count} enemigos de tipo {enemy.name}");
            for (int i = 0; i < count; i++)
            {
                Transform spawn = SelectSpawnPoint();

                GameObject newEnemy = Instantiate(enemy, spawn);

                AddEnemy(newEnemy);

                yield return new WaitForSeconds(0.5f);
            }
        }

        public void AddEnemy(GameObject enemy)
        {
            activeEnemies.Add(enemy);
            enemy.GetComponent<PVEEnemyData>().OnDeath += () => activeEnemies.Remove(enemy);
        }

        private Transform SelectSpawnPoint()
        {
            Transform playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

            List<Transform> nearSpawnPoints = new List<Transform>();

            foreach (Transform spawnPoint in spawnPoints)
            {
                float distance = Vector3.Distance(spawnPoint.position, playerTransform.position);

                if (distance <= maxDistance)
                {
                    nearSpawnPoints.Add(spawnPoint);
                }
            }

            if (nearSpawnPoints.Count > 0)
            {
                return nearSpawnPoints[Random.Range(0, nearSpawnPoints.Count)];
            }
            else
            {
                Debug.LogWarning("No hay spawns cercanos, seleccionando uno aleatorio.");
                return spawnPoints[Random.Range(0, spawnPoints.Count)];
            }
        }
    }
}


