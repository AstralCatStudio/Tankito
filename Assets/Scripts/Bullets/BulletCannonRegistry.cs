using System.Collections.Generic;
using UnityEngine;

namespace Tankito
{
    public class BulletCannonRegistry : Singleton<BulletCannonRegistry>
    {
        [SerializeField]
        public GameObject m_bulletPrefab;
        [SerializeField]
        private BulletProperties m_baseBulletProperties;
        public BulletProperties BaseProperties { get => m_baseBulletProperties; }
        private Dictionary<ulong, BulletCannon> m_cannons = new Dictionary<ulong, BulletCannon>();

        public BulletCannon this[ulong clientId]
        {
            get => m_cannons[clientId];
            set => m_cannons[clientId] = value;
        }
    }
}