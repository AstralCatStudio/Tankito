using System;
using System.Collections.Generic;
using Tankito.Netcode.Simulation;
using UnityEngine;
using UnityEngine.Pool;
using Unity.Netcode;

namespace Tankito.SinglePlayer
{
    // Me ha dado perecita hacerlo genérico sorry, aunque este lo es para GameObjects
    public class SinglePlayerBulletPool : Singleton<SinglePlayerBulletPool>
    {
        [SerializeField] private GameObject m_prefab;
        [SerializeField] private int m_prewarmCount;
        private ObjectPool<GameObject> m_pool;
        private bool DEBUG;

        [ContextMenu("InitializePool")]
        public void Start()
        {
            InitializePool(m_prefab, m_prewarmCount);
        }

        public void InitializePool(GameObject prefab, int prewarmCount)
        {
            GameObject CreateFunc()
            {
                // We parent the spawned prefab to our transform, to make sure that it isn't loaded onto another scene (because of additive scene loading);
                var pooleableObject = Instantiate(prefab, transform);
                if (DEBUG) Debug.Log($"CreateFunc called on {prefab}, instantiated {pooleableObject}");
                return pooleableObject;
            }

            void ActionOnGet(GameObject pooleableObject)
            {
                if (DEBUG) Debug.Log($"ActionOnGet called on {pooleableObject}");
                pooleableObject.gameObject.SetActive(true);
            }

            void ActionOnRelease(GameObject pooleableObject)
            {
                if (DEBUG) Debug.Log($"ActionOnRelease called on {pooleableObject}");
                pooleableObject.gameObject.SetActive(false);
            }

            void ActionOnDestroy(GameObject pooleableObject)
            {
                if (DEBUG) Debug.Log($"ActionOnDestroy called on {pooleableObject}");
                Destroy(pooleableObject.gameObject);
            }

            // Create the pool
            m_pool = new ObjectPool<GameObject>(CreateFunc, ActionOnGet, ActionOnRelease, ActionOnDestroy, defaultCapacity: prewarmCount);

            // Populate the pool
            var prewarmObjects = new List<GameObject>();
            for (var i = 0; i < prewarmCount; i++)
            {
                var gotInstance = m_pool.Get();
                prewarmObjects.Add(gotInstance);
            }

            foreach (var pooleableObject in prewarmObjects)
            {
                m_pool.Release(pooleableObject);
            }
        }


        public GameObject Get(Vector2 position, Vector2 rotation)
        {
            float rotationDeg = Mathf.Atan2(rotation.x, rotation.y);
            return Get(position, rotationDeg);
        }

        /// <summary>
        /// Gets and (if <paramref name="autoSpawn"/> is true) adds a new bullet object to the local <see cref="NetSimulationManager"/>.<br />
        /// If <paramref name="autoSpawn"/> is false, you must call <see cref="BulletSimulationObject.OnNetworkSpawn()"/> 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="ownerId"></param>
        /// <param name="simObjId"></param>
        /// <param name="autoSpawn"></param>
        /// <returns></returns>
        public GameObject Get(Vector2 position, float rotation)
        {
            var poolObj = m_pool.Get();
            poolObj.transform.SetPositionAndRotation(position, Quaternion.AngleAxis(rotation, Vector3.forward));

            return poolObj;
        }

        public void Release(GameObject pooleableObj)
        {
            m_pool.Release(pooleableObj);
        }
    }
}