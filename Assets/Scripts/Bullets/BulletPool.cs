using System;
using System.Collections.Generic;
using Tankito.Netcode.Simulation;
using UnityEngine;
using UnityEngine.Pool;
using Unity.Netcode;

namespace Tankito
{
    // Super facil de generalizar a un pool generico, porfavor a la minima q haga falta hacer otro pool, hagamoslo - Bernat
    public class BulletPool : Singleton<BulletPool>
    {
        [SerializeField] private GameObject m_bulletPrefab;
        [SerializeField] private int m_prewarmCount;
        private ObjectPool<BulletSimulationObject> m_pool;
        private bool DEBUG;

        [ContextMenu("InitializePool")]
        public void Start()
        {
            InitializePool(m_bulletPrefab, m_prewarmCount);
        }

        public void InitializePool(GameObject bulletPrefab, int prewarmCount)
        {
            BulletSimulationObject CreateFunc()
            {
                // We parent the spawned prefab to our transform, to make sure that it isn't loaded onto another scene (because of additive scene loading);
                var bullet = Instantiate(bulletPrefab, transform).GetComponent<BulletSimulationObject>();
                if (DEBUG) Debug.Log($"CreateFunc called on {bulletPrefab}, instantiated {bullet}");
                return bullet;
            }

            void ActionOnGet(BulletSimulationObject bulletObject)
            {
                if (DEBUG) Debug.Log($"ActionOnGet called on {bulletObject} *PRE-HASHING STEP (hash is stale from prev. use)");
                bulletObject.gameObject.SetActive(true);
            }

            void ActionOnRelease(BulletSimulationObject bulletObject)
            {
                if (DEBUG) Debug.Log($"ActionOnRelease called on {bulletObject}");
                bulletObject.gameObject.SetActive(false);
            }

            void ActionOnDestroy(BulletSimulationObject bulletObject)
            {
                if (DEBUG) Debug.Log($"ActionOnDestroy called on {bulletObject}");
                Destroy(bulletObject.gameObject);
            }

            // Create the pool
            m_pool = new ObjectPool<BulletSimulationObject>(CreateFunc, ActionOnGet, ActionOnRelease, ActionOnDestroy, defaultCapacity: prewarmCount);
            
            // Populate the pool
            var prewarmBullets = new List<BulletSimulationObject>();
            for (var i = 0; i < prewarmCount; i++)
            {
                var gotInstance = m_pool.Get();
                prewarmBullets.Add(gotInstance);
            }

            foreach (var bulletObject in prewarmBullets)
            {
                m_pool.Release(bulletObject);
            }
        }


        public BulletSimulationObject Get(Vector2 position, ulong ownerId, int tick, int spawnN)
        {   
            if (DEBUG) Debug.Log($"[{SimClock.TickCounter}]Get called, Arguments: pos({position}) | ownerId({ownerId}) | tick({tick}) | spawnN({spawnN})");

            var simObjId = SimExtensions.HashSimObj(ownerId, tick, spawnN);
            return Get(position, ownerId, simObjId);
        }
        public BulletSimulationObject Get(Vector2 position, ulong ownerId, int tick, int spawnN, ulong spawnerId)
        {
            var simObjId = SimExtensions.HashSimObj(spawnerId, tick, spawnN);
            return Get(position, ownerId, simObjId);
        }

        /// <summary>
        /// Gets and (if <paramref name="autoSpawn"/> is true) adds a new bullet object to the local <see cref="NetSimulationManager"/>.<br />
        /// If <paramref name="autoSpawn"/> is true the spawning is handled by simulation manager with the spawn queue.
        /// If it is false you must call <see cref="BulletSimulationObject.OnNetworkSpawn()" manually to add it to the local sim manager./> 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="ownerId"></param>
        /// <param name="simObjId"></param>
        /// <param name="autoSpawn"></param>
        /// <returns></returns>
        public BulletSimulationObject Get(Vector2 position, ulong ownerId, ulong simObjId, bool autoSpawn = true)
        {   
            if (DEBUG && !autoSpawn) Debug.Log($"[{SimClock.TickCounter}]Get called, Arguments: pos({position}) | ownerId({ownerId}) | simObjId({simObjId}) | autoSpawn({autoSpawn})");


            var bulletObj = m_pool.Get();
            var objRB = bulletObj.GetComponent<Rigidbody2D>();
            bulletObj.SetSimObjId(simObjId);
            // We dont use rigidbody transformations because they won't be changed until after the next physics update
            //objRB.transform.position = position;
            //objRB.rotation = rotation;
            bulletObj.transform.position = position;

            bulletObj.SetOwner(ownerId);
            bulletObj.GetComponent<BulletController>().InitializeProperties();
            
            if (autoSpawn)
            {
                if (NetworkManager.Singleton.IsServer)
                {
                    ServerSimulationManager.Instance.QueueForSpawn(bulletObj);
                }
                else
                {
                    ClientSimulationManager.Instance.QueueForSpawn(bulletObj);
                }
            }
            
            return bulletObj;
        }

        public void Release(BulletSimulationObject bullet)
        {
            m_pool.Release(bullet);
        }
    }
}