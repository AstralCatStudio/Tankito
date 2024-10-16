using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tankito.Netcode;

namespace Tankito
{
    public class ClockManager : MonoBehaviour
    {
        // La razon de que exista este script y ademas como singleton es para que todos los demas scripts tengan acceso al respectivo reloj de la instancia de juego
        
        // Lo he puesto enganchado al GameManager por ponerlo en un lugar localizable y persistente,
        // pero probablemente deberia ir en su propio GameObject de modo que se cree para usarse solo
        // durante el scope de una partida (contemplay AdditiveLoading para la escena de combate...)
        public const int SERVER_SIMULATION_TICKRATE = 60;
        public const float SERVER_SIMULATION_DELTA_TIME = 1f/SERVER_SIMULATION_TICKRATE;
        //public const int INPUT_SENDING_TICKRATE = 15;

        public static ClockManager Instance;
        public static Clock simulationClock; // Para la simulacion con lockstep
        
        // Pensandolo mejor probablemente tiene sentido aglutinar la maxima cantidad de informacion por RPC posible para ahorrar overhead y simplificar el codigo... Lo dejo como posible opcion en un futuro...
        //public static Clock inputSendClock; // Para no saturar la red mandando RPCs de input al polling rate de nuestra I/O


        // Start is called before the first frame update
        void Start()
        {
            if (ClockManager.Instance != null)
            {
                Destroy(this);
                return;
            }

            ClockManager.Instance = this;
            simulationClock = new Clock(SERVER_SIMULATION_TICKRATE);
            
            //inputSendClock = new Clock(INPUT_SENDING_TICKRATE, Clock.TickTriggerMode.Auto);
        }

        // Update is called once per frame
        void Update()
        {
            simulationClock.Update(Time.deltaTime);
        }
        //     inputSendClock.Update(Time.deltaTime);
        // }
// 
        // public void StartClocks()
        // {
        //     simulationClock.Start();
        //     inputSendClock.Start();
        // }
// 
        // public void StopClocks()
        // {
        //     simulationClock.Stop();
        //     inputSendClock.Stop();
        // }
    }

}
