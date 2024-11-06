using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tankito.Netcode;
using Unity.Netcode;
using Tankito.Netcode.Simulation;

namespace Tankito
{
    public class ClockManager : Singleton<ClockManager>
    {
        // La razon de que exista este script y ademas como singleton es para que todos los demas scripts tengan acceso al respectivo reloj de la instancia de juego
        
        // Lo he puesto enganchado al GameManager por ponerlo en un lugar localizable y persistente,
        // pero probablemente deberia ir en su propio GameObject de modo que se cree para usarse solo
        // durante el scope de una partida (contemplay AdditiveLoading para la escena de combate...)
        //public const int SERVER_SIMULATION_TICKRATE = 60;
        //public const float SERVER_SIMULATION_DELTA_TIME = 1f/SERVER_SIMULATION_TICKRATE;
        //public const int INPUT_SENDING_TICKRATE = 15;

        //public Clock simulationClock; // Para la simulacion con lockstep

        // Pensandolo mejor probablemente tiene sentido aglutinar la maxima cantidad de informacion por RPC posible para ahorrar overhead y simplificar el codigo... Lo dejo como posible opcion en un futuro...
        //public Clock inputSendClock; // Para no saturar la red mandando RPCs de input al polling rate de nuestra I/O


        // PROVISIONAL: Hasta que hagamos funcionar nuestro propio clock con los metodos Update()
        float m_time;
        [SerializeField]
        private int m_tickCounter;
        public static int TickCounter { get => Instance.m_tickCounter; }
        private float m_simulationDeltaTime;
        [SerializeField]
        private bool m_active;

        public bool Active { get => m_active; }
        public static float SimDeltaTime { get => Instance.m_simulationDeltaTime; set => Instance.m_simulationDeltaTime = value; }

        public delegate void UpdateSimulation();
        public static event UpdateSimulation OnTick;

        protected override void Awake()
        {
            base.Awake();
            //ClockManager.Instance = this;
            //simulationClock = new Clock(SERVER_SIMULATION_TICKRATE);
            
            //inputSendClock = new Clock(INPUT_SENDING_TICKRATE, Clock.TickTriggerMode.Auto);
            m_time = 0;
            m_tickCounter = 0;
            m_active = false;
            SimDeltaTime = Time.fixedDeltaTime;

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
            //simulationClock.Update(Time.deltaTime);
            if (!m_active) return;

            m_time += Time.deltaTime;
            if (m_time >= Time.fixedDeltaTime)
            {
                m_time -= Time.fixedDeltaTime;
                m_tickCounter++;
                OnTick?.Invoke();
            }
        }

        internal void StartClock()
        {
            m_time = 0;
            m_active = true;
        }

        internal void StopClock()
        {
            m_active = false;
        }

        internal void ResetClock()
        {
            m_tickCounter = 0;
            m_time = 0;
        }

        [ClientRpc]
        static public void StopClockClientRpc()
        {
            Instance.StopClock();
        }

        [ClientRpc]
        static public void ResetClockClientRpc()
        {
            Instance.ResetClock();
        }
    }

}
