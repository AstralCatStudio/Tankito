using System;
using Tankito.Netcode.Simulation;
using UnityEngine;
using UnityEngine.Pool;

namespace Tankito
{
    // Super facil de generalizar a un pool generico, porfavor a la minima q haga falta hacer otro pool, hagamoslo - Bernat
    public class BulletPool : Singleton<BulletPool>
    {
        [SerializeField] private GameObject m_bulletPrefab;
        [SerializeField] private int m_prewarmCount;
        private ObjectPool<GameObject> m_pool;
        private bool DEBUG;

        public void Start()
        {
            InitializePool(m_bulletPrefab, m_prewarmCount);
        }

        public void InitializePool(GameObject bulletPrefab, int prewarmCount)
        {
            GameObject CreateFunc()
            {
                // We parent the spawned prefab to our transform, to make sure that it isn't loaded onto another scene (because of additive scene loading);
                var no = Instantiate(bulletPrefab, transform).GetComponent<GameObject>();
                if (DEBUG) Debug.Log($"CreateFunc called on {bulletPrefab}, instantiated {no}");
                return no;
            }

            void ActionOnGet(GameObject bulletObject)
            {
                if (DEBUG) Debug.Log($"ActionOnGet called on {bulletObject}");
                bulletObject.SetActive(true);
            }

            void ActionOnRelease(GameObject bulletObject)
            {
                if (DEBUG) Debug.Log($"ActionOnRelease called on {bulletObject}");
                bulletObject.SetActive(false);
            }

            void ActionOnDestroy(GameObject bulletObject)
            {
                if (DEBUG) Debug.Log($"ActionOnDestroy called on {bulletObject}");
                Destroy(bulletObject.gameObject);
            }

            m_pool = new ObjectPool<GameObject>(CreateFunc, ActionOnGet, ActionOnRelease, ActionOnDestroy, defaultCapacity: prewarmCount);
        }


        public BulletSimulationObject Get(Vector2 position, Vector2 rotation, ulong ownerId, int tick, int spawnN)
        {   
            float rotationDeg = Mathf.Atan2(rotation.x, rotation.y);

            return Get(position, rotationDeg, ownerId, tick, spawnN);
        }

        public BulletSimulationObject Get(Vector2 position, float rotation, ulong ownerId, int tick, int spawnN)
        {   
            var gameObj = m_pool.Get();
            var objRB = gameObj.GetComponent<Rigidbody2D>();
            objRB.position = position;
            objRB.rotation = rotation;
            var bullet = objRB.GetComponent<BulletSimulationObject>();

            bullet.GenerateSimObjId(ownerId, tick, spawnN);
            
            return bullet;
        }

        public void Release(BulletSimulationObject bullet)
        {
            m_pool.Release(bullet.gameObject);
        }
    }
}