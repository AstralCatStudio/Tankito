using System.Collections.Generic;
using Tankito.Utils;
using Unity.Netcode;
using UnityEngine;

namespace Tankito.Netcode.Simulation
{
    public class SnapshotJitterBuffer : Singleton<SnapshotJitterBuffer>
    {
        private int m_bufferCount;
        public int SnapshotTimestamp { get => m_latestSnapshot.timestamp; }
        private SimulationSnapshot m_latestSnapshot;
        private double m_bufferedTime;
        private double TimeToBuffer { get => SimulationParameters.SNAPSHOT_JITTER_BUFFER_TIME; }


        protected override void Awake()
        {
            Debug.Log("SnapshotJitterBuffer Awake()");
            if (NetworkManager.Singleton.IsServer)
            {
                Destroy(this);
            }
            
            base.Awake();

            m_latestSnapshot = default;
        }

        void Update()
        {
            m_bufferedTime += Time.deltaTime;

                        
            if (m_bufferCount > 0 && m_bufferedTime >= TimeToBuffer)
            {
                m_bufferedTime = 0;
                ClientSimulationManager.Instance.EvaluateForReconciliation(m_latestSnapshot);
                m_latestSnapshot = default;
                m_bufferCount = 0;
            }
        }


        public bool AddSnapshot(SimulationSnapshot newSnapshot)
        {
            if (!newSnapshot.Equals(default) && newSnapshot.timestamp > m_latestSnapshot.timestamp)
            {
                m_bufferCount++;
                m_latestSnapshot = newSnapshot;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}