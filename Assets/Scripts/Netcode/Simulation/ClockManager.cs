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
    public class ClockManager : Singleton<ClockManager>
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
        public static float SimDeltaTime { get => Instance.m_throttleDeltaTime; }
        private float m_simulationDeltaTime;
        private float m_throttleDeltaTime;

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
            m_throttleDeltaTime = m_simulationDeltaTime;

            if (NetworkManager.Singleton.IsServer)
            {
                OnTick += ServerSimulationManager.Instance.Simulate;
            }
            
            if (NetworkManager.Singleton.IsClient)
            {
                OnTick += ClientSimulationManager.Instance.Simulate;
            }
        }


        // Update is called once per frame
        void Update()
        {
            if (!m_active) return;

            m_tickTimer += Time.deltaTime;
            if (m_tickTimer >= m_throttleDeltaTime)
            {
                m_tickTimer -= m_throttleDeltaTime;
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

        internal void ThrottleClock(int throttleTicks)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                Debug.LogWarning("SERVER MUSTN'T Throttle");
                return;
            }

            m_throttleDeltaTime = m_simulationDeltaTime + throttleTicks/TICKS_PER_SECOND;
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
