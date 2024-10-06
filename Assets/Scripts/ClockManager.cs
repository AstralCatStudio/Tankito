using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tankito.Netcode;

namespace Tankito
{
    public class ClockManager : MonoBehaviour
    {
        public const uint SIMULATION_TICKRATE = 60;
        public const uint INPUT_SENDING_TICKRATE = 15;
        public static Clock simulationClock; // Para la simulacion con lockstep
        public static Clock inputSendClock; // Para no saturar la red mandando RPCs de input al polling rate de nuestra I/O

        // Start is called before the first frame update
        void Start()
        {
            simulationClock = new Clock(SIMULATION_TICKRATE);
            inputSendClock = new Clock(INPUT_SENDING_TICKRATE, Clock.TickTriggerMode.Auto);
        }

        // Update is called once per frame
        void Update()
        {
            inputSendClock.Update(Time.deltaTime);
            simulationClock.Update(Time.deltaTime);
        }
    }

}
