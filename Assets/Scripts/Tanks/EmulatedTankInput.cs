using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tankito.Utils;
using UnityEngine;

namespace Tankito.Netcode.Simulation
{
    public class EmulatedTankInput : MonoBehaviour, ITankInput
    {
        private const int INPUT_CACHE_SIZE = 256;
        private CircularBuffer<InputPayload> m_inputBuffer = new CircularBuffer<InputPayload>(INPUT_CACHE_SIZE);
        private float m_attenuationSeconds = 0.5f;
        [SerializeField] private int m_attenuationTicks;
        private InputPayload m_currentInput;
        //private InputPayload m_lastReceivedInput;

        [SerializeField] private bool DEBUG = false;


        private int m_inputReplayTick = NO_REPLAY;
        private const int NO_REPLAY = -1;

        public ulong ClientId { get => ClientSimulationManager.Instance.emulatedInputTanks.First(compIdPair => compIdPair.Value == this).Key; }



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
            if ((SimClock.TickCounter > INPUT_CACHE_SIZE &&
                inputWindow.First() < (SimClock.TickCounter - INPUT_CACHE_SIZE)) ||
                inputWindow.Last() > SimClock.TickCounter + INPUT_CACHE_SIZE)
            {
                if (DEBUG) Debug.Log($"[{SimClock.TickCounter}]Discarded InputWindow[{inputWindow.First().timestamp}-{inputWindow.Last().timestamp}]");
                return; // En caso de que el input sea muy viejo no lo guardamos, porque puede machacarnos datos nuevos de prediccion, y viceversa.
            }

            foreach(var input in inputWindow)
            {
                m_inputBuffer[input.timestamp] = input;
            }

            if (DEBUG)
            {
                Debug.Log($"[{SimClock.TickCounter}]EmulatedTankInputBuffer({ClientId}): [{m_inputBuffer.MinBy(i => i.timestamp).timestamp}-{m_inputBuffer.MaxBy(i => i.timestamp).timestamp}]");
            }
        }

        public InputPayload GetInput()
        {
            InputPayload newInput;
            bool inputFound = m_inputBuffer.TryGet(out newInput, SimClock.TickCounter);

            if(inputFound && newInput.timestamp == SimClock.TickCounter)
            {
                m_currentInput = newInput;

                if (DEBUG) Debug.Log($"[{SimClock.TickCounter}] Tank({ClientId}): Input{m_currentInput}");
            }
            else
            {
                m_currentInput = InterpolateInputAt(SimClock.TickCounter);
            }

            if (m_inputReplayTick != NO_REPLAY)
            {
                // Input Replay Mode
                m_inputReplayTick++;
            }

            return m_currentInput;
        }

        public InputPayload GetCurrentInput()
        {
            return m_currentInput;
        }

        private InputPayload InterpolateInputAt(int tick)
        {
            InputPayload interpInput;
            var pastInputs = m_inputBuffer.Where(x => (x <= tick) && (x >= tick-m_attenuationTicks)).ToList();
            var futureInputs = m_inputBuffer.Where( x => (x >= tick) && (x <= tick-m_attenuationTicks)).ToList();

            InputPayload prevInput = pastInputs.Count > 0 ? pastInputs.MaxBy(x => x.timestamp) : default(InputPayload);
            InputPayload nextInput = futureInputs.Count > 0 ? futureInputs.MinBy(x => x.timestamp) : default(InputPayload);

            if (DEBUG)
            {
                var debugLog = "TICK["+tick+"]\n";

                debugLog += "AllInputs-[";
                foreach(var i in m_inputBuffer)
                {
                    debugLog += i.timestamp + ((!i.Equals(pastInputs.Last())) ? ", " : "]\n");
                }

                debugLog += "PastInputs-[";
                foreach(var p in pastInputs)
                {
                    debugLog += p.timestamp + ((!p.Equals(pastInputs.Last())) ? ", " : "]\n");
                }

                debugLog += "FutureInputs-[";
                foreach(var f in futureInputs)
                {
                    debugLog += f.timestamp + ((!f.Equals(futureInputs.Last())) ? ", " : "]\n");
                }
                debugLog += "\n prevInput-" + prevInput + "\n nextInput-" + nextInput;

                Debug.Log(debugLog);
            }

            bool havePrevInput = !prevInput.Equals(default(InputPayload));
            bool haveNextInput = !nextInput.Equals(default(InputPayload));

            if(!havePrevInput && haveNextInput)
            {
                prevInput.timestamp = nextInput.timestamp - m_attenuationTicks;
                if (DEBUG) Debug.Log($"[{SimClock.TickCounter}]EmulatedInput - Interpolation mode: Backwards extrapolation. From {prevInput} to {nextInput}");
            }
            else if (havePrevInput && !haveNextInput)
            {
                nextInput.timestamp = prevInput.timestamp + m_attenuationTicks;
                if (DEBUG) Debug.Log($"[{SimClock.TickCounter}]EmulatedInput - Interpolation mode: Forwards extrapolation. From {prevInput} to {nextInput}");
            }
            else if (!havePrevInput && !haveNextInput)
            {
                if (DEBUG) Debug.Log($"[{SimClock.TickCounter}]EmulatedInput - Interpolation mode: NONE (default value): {default}");
                return default;
            }
            else
            {
                if (DEBUG) Debug.Log($"[{SimClock.TickCounter}]EmulatedInput - Interpolation mode: Full interpolation. From {prevInput} to {nextInput}");
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
