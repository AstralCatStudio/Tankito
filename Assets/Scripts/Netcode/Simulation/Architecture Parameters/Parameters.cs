
using System;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

namespace Tankito.Netcode
{
    public /*static*/ class Parameters : Singleton<Parameters>
    {

        /// <summary>
        /// Number of snapshot ticks that'll be stored in a circular buffer in order to check prediciton
        /// </summary>
        public static int SNAPSHOT_BUFFER_SIZE { get => Instance.C_Buffer_Size; }
        public static int CLIENT_INPUT_WINDOW_SIZE { get => SERVER_IDEAL_INPUT_BUFFER_SIZE; }
        public static int SERVER_IDEAL_INPUT_BUFFER_SIZE { get => Instance.C_Buffer_Size; }
        public static double SNAPSHOT_JITTER_BUFFER_TIME { get => 0.02; }

        private int C_Buffer_Size { get => (int)(Worst_Case_Latency/SIM_DELTA_TIME) + 1; }
        

        [SerializeField] double Median_Latency = 0.060;
        [SerializeField] double Worst_Case_Latency = 0.300;
        /// <summary>
        /// Latency of ping (HRTT).
        /// </summary>
        public static double CURRENT_LATENCY { get =>  Math.Abs((NetworkManager.Singleton.ServerTime - NetworkManager.Singleton.LocalTime).Time); }

        [SerializeField] int Sim_Tick_Rate = 30;
        [SerializeField] bool DEBUG_LATENCY = false;
        public static int SIM_TICK_RATE { get => Instance.Sim_Tick_Rate; }
        /// <summary>
        /// Physics simulation time step. (duration of 1 tick)
        /// </summary>
        public static double SIM_DELTA_TIME { get => 1.0/Instance.Sim_Tick_Rate; }

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }
        
        void Update()
        {
            if (DEBUG_LATENCY)
            {
                Debug.Log("Average Latency (ping/HRTT): "+ CURRENT_LATENCY);
            }
        }
    }
}