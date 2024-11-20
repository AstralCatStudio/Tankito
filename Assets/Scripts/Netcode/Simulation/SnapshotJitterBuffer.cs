using System.Collections.Generic;
using Tankito.Utils;
using Unity.Netcode;
using UnityEngine;

namespace Tankito.Netcode.Simulation
{
    public class SnapshotJitterBuffer : Singleton<SnapshotJitterBuffer>
    {
        const float AUTH_SNAPSHOT_JITTER_BUFFER_SIZE = 0.15f;

        private SimulationSnapshot m_latestSnapshot;
        private double m_bufferedTime;
        private double m_timeToBuffer;

        public double BufferTime { get => m_timeToBuffer; }

        protected override void Awake()
        {
            Debug.Log("SnapshotJitterBuffer Awake()");
            if (NetworkManager.Singleton.IsServer)
            {
                Destroy(this);
            }
            
            base.Awake();

            m_timeToBuffer = AUTH_SNAPSHOT_JITTER_BUFFER_SIZE;
            m_latestSnapshot = default;
        }

        void Update()
        {
            m_bufferedTime += Time.deltaTime;
            SimulationSnapshot newSnapshot;
            if(GetSnapshot(out newSnapshot))
            {
                m_bufferedTime = 0;
                ClientSimulationManager.Instance.EvaluateForReconciliation(newSnapshot);
            }
        }

        public void SetBufferTime(double seconds)
        {
            m_timeToBuffer = seconds;
        }

        public bool AddSnapshot(SimulationSnapshot newSnapshot)
        {
            if (!newSnapshot.Equals(default) && newSnapshot.timestamp > m_latestSnapshot.timestamp)
            {
                m_latestSnapshot = newSnapshot;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool GetSnapshot(out SimulationSnapshot snapshot)
        {
            if (m_bufferedTime >= m_timeToBuffer && !m_latestSnapshot.Equals(default))
            {
                snapshot = m_latestSnapshot;
                m_latestSnapshot = default;
                return true;
            }
            else
            {
                snapshot = default;
                return false;
            }
        }

        #if UNITY_EDITOR

        [ContextMenu("Increase Jitter Buffer Size")]
        public void IncreaseJitterBufferSize()
        {
            double bufferTicks = BufferTime + 0.005;
            this.SetBufferTime(bufferTicks);
            Debug.Log($"Set Jitter Buffer to : {bufferTicks}ticks");
        }

        [ContextMenu("Decrease Jitter Buffer Size")]
        public void DecreaseJitterBufferSize()
        {
            double bufferTime = BufferTime - 0.005;

            this.SetBufferTime(bufferTime);
            Debug.Log($"Set Jitter BufferTime to : {bufferTime}s");
        }

        #endif
    }
}