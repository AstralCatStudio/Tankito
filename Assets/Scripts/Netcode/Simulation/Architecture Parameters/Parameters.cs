
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
        public static int SERVER_IDEAL_INPUT_BUFFER_SIZE { get => (int)(Instance.Worst_Case_Delay/SIM_DELTA_TIME + SIM_DELTA_TIME); }
        public static double SNAPSHOT_JITTER_BUFFER_TIME { get => 0.02; }

        private int C_Buffer_Size { get => (int)(Worst_Case_Delay/SIM_DELTA_TIME); }
        

        [SerializeField] double Median_Delay = 60;
        [SerializeField] double Worst_Case_Delay = 300;

        [SerializeField] int Sim_Tick_Rate = 30;
        [SerializeField] bool DEBUG = false;
        public static int SIM_TICK_RATE { get => Instance.Sim_Tick_Rate; }
        private static double SIM_DELTA_TIME { get => 1.0/Instance.Sim_Tick_Rate; }

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }
        
        void Update()
        {
            if (DEBUG)
            {
                Debug.Log("Average Latency (ping/HRTT): "+ ((NetworkManager.Singleton.ServerTime - NetworkManager.Singleton.LocalTime).Time - NetworkManager.Singleton.LocalTime.FixedDeltaTime));
            }
        }
    }
}