using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tankito;
using Tankito.Netcode;
using Tankito.Netcode.Simulation;
using Tankito.Utils;
using UnityEngine;

namespace Tankito
{
    public class EmulatedTankInput : MonoBehaviour, ITankInput
    {
        private const int INPUT_CACHE_SIZE = 256;
        private CircularBuffer<InputPayload> m_inputBuffer = new CircularBuffer<InputPayload>(INPUT_CACHE_SIZE);
        [SerializeField] private float m_attenuationSeconds = 3;
        [SerializeField] private int m_attenuationTicks;
        private InputPayload m_currentInput;
        //private InputPayload m_lastReceivedInput;

        [SerializeField] private bool DEBUG = false;


        private int m_inputReplayTick = NO_REPLAY;
        private const int NO_REPLAY = -1;



        void Awake()
        {
            m_attenuationTicks = (int)(m_attenuationSeconds/SimClock.SimDeltaTime);
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
            if (inputWindow.First() < (SimClock.TickCounter - INPUT_CACHE_SIZE) || inputWindow.Last() > SimClock.TickCounter + INPUT_CACHE_SIZE)
            {
                if (DEBUG) Debug.Log($"Discarded InputWindow[{inputWindow.First().timestamp}-{inputWindow.Last().timestamp}]");
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
                bool inputInterpolation;

                if(!m_inputBuffer.TryGet(out newInput, SimClock.TickCounter) && newInput.timestamp != SimClock.TickCounter)
                {
                    m_currentInput = InterpolateInputAt(SimClock.TickCounter);
                    inputInterpolation = true;
                }
                else
                {
                    m_currentInput = newInput;
                    inputInterpolation = false;
                }

                if (DEBUG)
                {
                    if (ClientSimulationManager.Instance.emulatedInputTanks.ContainsValue(this))
                    {
                        var emulatedInputClientId = ClientSimulationManager.Instance.emulatedInputTanks.First(compIdPair => compIdPair.Value == this);
                        Debug.Log($"[{SimClock.TickCounter}]GetInput(EmulatedClient[{emulatedInputClientId}]){(inputInterpolation ? " INTERPOLATED" : "")}: {m_currentInput}");
                    }
                    else
                    {
                        throw new InvalidOperationException("EmulatedTankInput component is not bound to a clientId!");
                    }
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

        private InputPayload InterpolateInputAt(int tick)
        {
            InputPayload interpInput;
            var pastInputs = m_inputBuffer.Where(x => x.timestamp <= tick).ToList();
            var futureInputs = m_inputBuffer.Where( x => x.timestamp >= tick).ToList();
            InputPayload prevInput = pastInputs.Count > 0 ? pastInputs.MaxBy(x => x.timestamp) : default(InputPayload);
            InputPayload nextInput = futureInputs.Count > 0 ? futureInputs.MinBy(x => x.timestamp) : default(InputPayload);

            bool havePrevInput = !prevInput.Equals(default(InputPayload));
            bool haveNextInput = !nextInput.Equals(default(InputPayload));

            if(!havePrevInput && haveNextInput)
            {
                prevInput.timestamp = nextInput.timestamp - m_attenuationTicks;
                if (DEBUG) Debug.Log("EmulatedInput - Interpolation mode: Backwards extrapolation");
            }
            else if (havePrevInput && !haveNextInput)
            {
                nextInput.timestamp = prevInput.timestamp + m_attenuationTicks;
                if (DEBUG) Debug.Log("EmulatedInput - Interpolation mode: Forwards extrapolation");
            }
            else if (!havePrevInput && !haveNextInput)
            {
                if (DEBUG) Debug.Log("EmulatedInput - Interpolation mode: NONE (default value)");
                return default;
            }
            else
            {
                if (DEBUG) Debug.Log($"EmulatedInput - Interpolation mode: Full interpolation. From {prevInput} to {nextInput}");
            }

            interpInput = prevInput;

            interpInput.Interpolate(nextInput, SimClock.TickCounter);

            return interpInput;
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
