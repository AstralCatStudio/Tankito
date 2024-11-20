

using System.Collections.Generic;
using Tankito.Utils;

namespace Tankito.Netcode.Simulation
{
    public class SnapshotAccumulator : Singleton<SnapshotAccumulator>
    {
        private SimulationSnapshot m_latestSnapshot;
        private int m_bufferedTicks;
        public int BufferSize { get => m_ticksToBuffer; }
        private int m_ticksToBuffer;
        
        public SnapshotAccumulator(int ticksToBuffer)
        {
            m_ticksToBuffer = ticksToBuffer;
        }

        public void SetBufferSize(int ticks)
        {
            m_ticksToBuffer = ticks;
        }

        public bool AddSnapshot(SimulationSnapshot newSnapshot)
        {
            if (newSnapshot.timestamp > m_latestSnapshot.timestamp)
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
            m_bufferedTicks++;

            if (m_bufferedTicks > m_ticksToBuffer && !m_latestSnapshot.Equals(default))
            {
                m_bufferedTicks = 0;
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
    }
}