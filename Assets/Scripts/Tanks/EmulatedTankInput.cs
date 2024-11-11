using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tankito;
using Tankito.Netcode;
using Tankito.Utils;
using UnityEngine;

namespace Tankito
{
    public class EmulatedTankInput : MonoBehaviour, ITankInput
    {
        private const int INPUT_CACHE_SIZE = 256;
        private CircularBuffer<InputPayload> m_inputBuffer = new CircularBuffer<InputPayload>(INPUT_CACHE_SIZE);
        [SerializeField] private float m_attenuationSeconds = 3;
        private InputPayload m_currentInput;
        //private InputPayload m_lastReceivedInput;


        private int m_inputReplayTick = NO_REPLAY;
        private const int NO_REPLAY = -1;

        float m_decayFactor;

        void Start()
        {
            m_decayFactor = 1f-SimClock.SimDeltaTime/m_attenuationSeconds;
        }

    public void ReceiveInputWindow(InputPayload[] inputWindow)
    {
        // int i = Array.FindIndex(inputWindow, m => m.timestamp == m_lastReceivedInput.timestamp); // Esto sencillamente no va a pasar. Por que ibas a tener la marca temporal del ultimo input recibido en la nueva ventana? Hay que asumir que los paquetes no llegan en orden.
        // if (i == -1) i = 0;
        // else i++;
        // for (; i < inputWindow.Length; i++)
        // {
        //     m_inputBuffer.Add(inputWindow[i], inputWindow[i].timestamp);
        // }
        // if(m_lastReceivedInput.timestamp < inputWindow[i-1].timestamp)
        // {
        //     m_lastReceivedInput = inputWindow[i - 1];
        // }
        if (inputWindow[0] < (SimClock.TickCounter - 256) || inputWindow.Last() > SimClock.TickCounter + 256)
        {
            return; // En caso de que el input sea muy viejo no lo guardamos, porque puede machacarnos datos nuevos de prediccion, y viceversa.
        }

        foreach(var input in inputWindow)
        {
            m_inputBuffer[input.timestamp] = input;
        }
    }

    public InputPayload GetInput()
    {
        if (m_inputReplayTick == NO_REPLAY)
        {
            InputPayload newInput;
            if(!m_inputBuffer.TryGet(out newInput, SimClock.TickCounter) && newInput.timestamp != SimClock.TickCounter)
            {
                AttenuateInput();
            }
            return m_currentInput;
        }
        else
        {
            // Input Replay Mode
            var replayedInput = m_inputBuffer.Get(m_inputReplayTick);
            m_inputReplayTick++;
            return replayedInput;
        }
    }

    private void AttenuateInput()
    {
        m_currentInput.moveVector *= m_decayFactor;
        m_currentInput.action = TankAction.None;
        //m_inputBuffer.Add(attenuatedInput, attenuatedInput.timestamp + 1);// Mejor no ensuciar el buffer de referencia para la reconciliacion con inputs confabulados,
                                                                            // y asi preservamos datos de simulacion legitimos.
    }

        public void StartInputReplay(int timestamp)
        {
            m_inputReplayTick = timestamp;
        }

        public int StopInputReplay()
        {
            var lastReplayTick = m_inputReplayTick;
            m_inputReplayTick = NO_REPLAY;
            return lastReplayTick;
        }
    }
}
