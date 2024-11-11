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
        private InputPayload m_lastReceivedInput;

        private int m_inputReplayTick = NO_REPLAY;
        private const int NO_REPLAY = -1;

        float decayFactor;

        void Start()
        {
            decayFactor = 1f-SimClock.SimDeltaTime/m_attenuationSeconds;
        }

    public void ReceiveInputWindow(InputPayload[] inputWindow)
    {
        int i = Array.FindIndex(inputWindow, m => m.timestamp == m_lastReceivedInput.timestamp);
        if (i == -1) i = 0;
        else i++;
        for (; i < inputWindow.Length; i++)
        {
            m_inputBuffer.Add(inputWindow[i], inputWindow[i].timestamp);
        }
        if(m_lastReceivedInput.timestamp < inputWindow[i-1].timestamp)
        {
            m_lastReceivedInput = inputWindow[i - 1];
        }
    }

    public InputPayload GetInput()
    {
        if (m_inputReplayTick == NO_REPLAY)
        {
            m_currentInput = m_inputBuffer.Get(SimClock.TickCounter);
            if(m_currentInput.timestamp >= m_lastReceivedInput.timestamp)
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
        InputPayload attenuatedInput = m_currentInput;
        attenuatedInput.moveVector *= decayFactor;
        attenuatedInput.action = TankAction.None;
        m_inputBuffer.Add(attenuatedInput, attenuatedInput.timestamp + 1);
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
