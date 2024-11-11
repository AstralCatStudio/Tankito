using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tankito.Netcode;
using Unity.Netcode;
using Tankito.Netcode.Simulation;
using System;
using NUnit.Framework;

namespace Tankito
{
    public class SimClock : Singleton<SimClock>
    {
        const int TICKS_PER_SECOND = 30;
        const float SIM_DELTA_TIME = 1f/TICKS_PER_SECOND;

        float m_tickTimer;
        [SerializeField]
        private int m_tickCounter;
        public static int TickCounter { get => Instance.m_tickCounter; }
        [SerializeField]
        private bool m_active;

        public bool Active { get => m_active; }
        public static float SimDeltaTime { get => Instance.m_simulationDeltaTime; }
        private float m_simulationDeltaTime;

        const int THROTTLE_COOLDOWN = 0;
        private int m_throttleCooldown; // In TPS (Ticks Per Second)
        private float m_averageThrottleTicks;
        private int m_throttleMessages;

        [SerializeField] private bool DEBUG;

        public delegate void UpdateSimulation();
        public static event UpdateSimulation OnTick;

        protected override void Awake()
        {
            base.Awake();
            //ClockManager.Instance = this;
            //simulationClock = new Clock(SERVER_SIMULATION_TICKRATE);
            
            //inputSendClock = new Clock(INPUT_SENDING_TICKRATE, Clock.TickTriggerMode.Auto);
            m_tickTimer = 0;
            m_tickCounter = 0;
            m_active = false;
            m_simulationDeltaTime = SIM_DELTA_TIME;
        }


        // Update is called once per frame
        void Update()
        {
            if (!m_active) return;

            m_tickTimer += Time.deltaTime;
            if (m_tickTimer >= m_simulationDeltaTime)
            {
                m_tickTimer -= m_simulationDeltaTime;
                m_tickCounter++;
                OnTick?.Invoke();
            }
        }

        internal void StartClock()
        {
            m_tickTimer = 0;
            m_active = true;
            AutoPhysics2DUpdate(false);
        }

        internal void StopClock()
        {
            m_active = false;
        }

        internal void ResumeClock()
        {
            m_active = true;
        }

        internal void ResetClock()
        {
            m_tickCounter = 0;
            m_tickTimer = 0;
        }

        internal void ThrottleClock(int throttleTicks, int serverTime)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                Debug.LogWarning("SERVER MUSTN'T Throttle");
                return;
            }

            float newTPS = TICKS_PER_SECOND+Mathf.Clamp(throttleTicks, -TICKS_PER_SECOND, TICKS_PER_SECOND-1);
            m_simulationDeltaTime = 1f/newTPS;

        }

        
        public void AutoPhysics2DUpdate(bool auto)
        {
            if (!auto)
            {
                Physics2D.simulationMode = SimulationMode2D.Script;
            }
            else
            {
                Physics2D.simulationMode = SimulationMode2D.FixedUpdate;
            }
        }
    }

}