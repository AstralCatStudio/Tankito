
using System;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

namespace Tankito.Netcode
{
    public /*static*/ class SimulationParameters : Singleton<SimulationParameters>
    {

        /// <summary>
        /// Number of snapshot ticks that'll be stored in a circular buffer in order to check prediciton
        /// </summary>
        public static int SNAPSHOT_BUFFER_SIZE { get => Instance.C_Buffer_Size; }
        public static int CLIENT_INPUT_WINDOW_SIZE { get => Instance.S_Buffer_Size; }
        public static int SERVER_IDEAL_INPUT_BUFFER_SIZE { get => Instance.S_Buffer_Size; }
        public static double WORST_CASE_LATENCY { get => Instance.Worst_Case_Latency; set => Instance.Worst_Case_Latency = value; }
        public static double SNAPSHOT_JITTER_BUFFER_TIME { get => Instance.Client_Jitter_Buffer_Time; }
        public static int CLIENT_MAX_DESYNC_COUNT { get => Instance.Client_Max_Desync_Count; }
        public static int SERVER_MAX_DESYNC_COUNT { get => Instance.Server_Max_Desync_Count; }

        private int S_Buffer_Size { get => Mathf.CeilToInt((float)(Worst_Case_Latency/SIM_DELTA_TIME)) + 1; }
        private int C_Buffer_Size { get => Mathf.CeilToInt((float)(Worst_Case_Latency*3/SIM_DELTA_TIME)) + 1; }
        

        [SerializeField] double Median_Latency = 0.16;
        public double MedianLatency => Median_Latency;
        [SerializeField] double Worst_Case_Latency = 0.400;
        public double WorstCaseLatency => Worst_Case_Latency;
        [SerializeField] double Client_Jitter_Buffer_Time = 0.02;
        [SerializeField] int Client_Max_Desync_Count = 5;
        [SerializeField]  int Server_Max_Desync_Count = 10;
        [SerializeField] int Sim_Tick_Rate = 30;
        public int SimTickRate => Sim_Tick_Rate;

        /// <summary>
        /// Latency of ping (HRTT).
        /// </summary>
        public static double CURRENT_LATENCY { get => NetworkManager.Singleton.IsConnectedClient ? Math.Abs((NetworkManager.Singleton.ServerTime - NetworkManager.Singleton.LocalTime).Time) : -1; }

        [SerializeField] bool DEBUG_LATENCY = false;
        [SerializeField] bool DEBUG_PARAMS = false;

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

        public void SetParams(float medianLatency, float worstCaseLatency, int simTickRate)
        {
            Median_Latency = medianLatency;
            Worst_Case_Latency = worstCaseLatency;
            Sim_Tick_Rate = simTickRate;
        }

        [SerializeField] float m_debugRate = 2f;
        float m_debugTimeAccum = 0f;
        
        void Update()
        {
            m_debugTimeAccum += Time.deltaTime;

            if ((int) m_debugTimeAccum > m_debugRate)
            {
                m_debugTimeAccum = 0;
                if (DEBUG_LATENCY)
                {
                    if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer)
                        Debug.Log("Average Latency (ping/HRTT): "+ (int)(CURRENT_LATENCY * 1000) + "ms");
                }
                
                if (DEBUG_PARAMS)
                {
                    Debug.Log($"PARAMS(WorstCase ping={(int)(WORST_CASE_LATENCY*1000)}ms): " +
                                "\nSNAPSHOT_BUFFER_SIZE: " + SNAPSHOT_BUFFER_SIZE +
                                "\nCLIENT_INPUT_WINDOW_SIZE: " + CLIENT_INPUT_WINDOW_SIZE +
                                "\nSERVER_IDEAL_INPUT_BUFFER_SIZE: " + SERVER_IDEAL_INPUT_BUFFER_SIZE +
                                "\nSNAPSHOT_JITTER_BUFFER_TIME: " + SNAPSHOT_JITTER_BUFFER_TIME);
                }
            }
        }
    }
}