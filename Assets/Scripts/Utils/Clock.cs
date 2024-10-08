using System;
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

        const int DEFAULT_TICKRATE = 60;
        /// <summary>
        /// In Ticks Per Second (t/s).
        /// </summary>
        public int TickRate { get => m_tickRate; }
        public int CurrentTick { get => m_currentTick; }
        public float TickPeriod { get => m_tickPeriod; }
        public TickTriggerMode Mode { get => m_mode; }
        public bool IsRunning { get => m_active; }
        
        private readonly int m_tickRate;
        private readonly float m_tickPeriod;
        private TickTriggerMode m_mode;
        private bool m_active;
        public TickDelegate OnTick;

        public delegate void TickDelegate();
        private int m_currentTick;
        private float time;

        public Clock(int tickRate = DEFAULT_TICKRATE, TickTriggerMode mode = TickTriggerMode.Auto, bool startRunning = false)
        {
            m_tickRate = tickRate;
            m_tickPeriod = 1f/tickRate;
            m_active = startRunning;
            OnTick = Tick;
        }

        public void Tick()
        {
            //Debug.Log("TICK");
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

        internal void Start()
        {
            m_active = true;
        }

        internal void Stop()
        {
            m_active = false;
        }
    }
}