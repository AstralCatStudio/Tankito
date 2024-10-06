using UnityEngine;

namespace Tankito.Netcode
{
    public class Clock
    {
        public enum TickTriggerMode
        {
            Auto,
            Manual
        }

        const uint DEFAULT_TICKRATE = 60;
        /// <summary>
        /// In Ticks Per Second (t/s).
        /// </summary>
        public uint TickRate { get => m_tickRate; }
        public uint CurrentTick { get => m_currentTick; }
        public float TickPeriod { get => m_tickPeriod; }
        public TickTriggerMode Mode { get => m_mode; }
        
        private readonly uint m_tickRate;
        private readonly float m_tickPeriod;
        private TickTriggerMode m_mode;
        public TickDelegate OnTick;

        public delegate void TickDelegate();
        private uint m_currentTick;
        private float time;

        public Clock(uint tickRate = DEFAULT_TICKRATE, TickTriggerMode mode = TickTriggerMode.Auto)
        {
            m_tickRate = tickRate;
            m_tickPeriod = 1f/tickRate;
            OnTick = Tick;
        }

        public void Tick()
        {
            m_currentTick++;
        }

        public void Update(float deltaTime)
        {
            time += deltaTime;
            if (m_mode == TickTriggerMode.Auto) TicksLeft();
        }

        /// <summary>
        /// If a tick is ready to be processed (1/tickRate has ellapsed) true is returned and OnTick delegate is called.
        /// </summary>
        /// <returns></returns>
        public bool TicksLeft()
        {
            if (time >= m_tickPeriod)
            {
                time -= m_tickPeriod;
                OnTick();
                return true;
            }
            return false;
        }

    }
}