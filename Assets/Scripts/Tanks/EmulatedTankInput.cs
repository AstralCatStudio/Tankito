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
        private int INPUT_CACHE_SIZE => SimulationParameters.SNAPSHOT_BUFFER_SIZE;
        private CircularBuffer<InputPayload> m_inputBuffer;
        private float m_attenuationSeconds = 0f;
        [SerializeField] private int m_attenuationTicks;
        private InputPayload m_currentInput;
        
        public InputPayload LastInput => m_inputReplayTick==NO_REPLAY ? m_inputBuffer.Get(SimClock.TickCounter) : m_inputBuffer.Get(m_inputReplayTick);

        [SerializeField] private bool DEBUG = false;


        private int m_inputReplayTick = NO_REPLAY;
        private const int NO_REPLAY = -1;

        public ulong ClientId { get => ClientSimulationManager.Instance.emulatedInputTanks.First(compIdPair => compIdPair.Value == this).Key; }



        void Awake()
        {
            m_attenuationTicks = (int)(m_attenuationSeconds/SimClock.SimDeltaTime);
        }

        void Start()
        {
            m_inputBuffer = new CircularBuffer<InputPayload>(INPUT_CACHE_SIZE);
        }

        public void ReceiveInputWindow(InputPayload[] inputWindow)
        {
            // if ((SimClock.TickCounter > INPUT_CACHE_SIZE &&
            //     inputWindow.Count() >= INPUT_CACHE_SIZE &&
            //     inputWindow.First() < (SimClock.TickCounter - INPUT_CACHE_SIZE)) ||
            //     inputWindow.Last() > SimClock.TickCounter + INPUT_CACHE_SIZE)
            // {
            //     if (DEBUG) Debug.Log($"[{SimClock.TickCounter}]Discarded InputWindow[{inputWindow.First().timestamp}-{inputWindow.Last().timestamp}]");
            //     return; // En caso de que el input sea muy viejo no lo guardamos, porque puede machacarnos datos nuevos de prediccion, y viceversa.
            // }

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
            if (m_inputReplayTick == NO_REPLAY)
            {
                return GetInputAt(SimClock.TickCounter);
            }
            else
            {
                // Input Replay Mode
                // We set the input replay tick as the next from the rollback tick because otherwise we will be replaying ipnut that
                // is lagged by 1 tick (since we don't have to simulate the incoming snapshot tick).
                m_inputReplayTick++;
                var newInput = GetInputAt(m_inputReplayTick);
                return newInput;
            }
        }

        private InputPayload GetInputAt(int tick)
        {
            if (m_inputBuffer.Select(input => input.timestamp).Contains(tick))
            {
                return m_inputBuffer.Get(tick);
            }
            else
            {
                return InterpolateInputAt(tick);
            }
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
                    debugLog += i.timestamp + ((!i.Equals(m_inputBuffer.Last())) ? ", " : "]\n");
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

            if (!havePrevInput && haveNextInput)
            {
                prevInput.timestamp = nextInput.timestamp - m_attenuationTicks;
                // Copy aim direction
                prevInput.aimVector = nextInput.aimVector;

                if (DEBUG) Debug.Log($"[{tick}]EmulatedInput - Interpolation mode: Backwards extrapolation. From {prevInput} to {nextInput}");
            }
            else if (havePrevInput && !haveNextInput)
            {
                nextInput.timestamp = prevInput.timestamp + m_attenuationTicks;
                // Copy aim direction
                nextInput.aimVector = prevInput.aimVector;

                if (DEBUG) Debug.Log($"[{tick}]EmulatedInput - Interpolation mode: Forwards extrapolation. From {prevInput} to {nextInput}");
            }
            else if (!havePrevInput && !haveNextInput)
            {
                InputPayload defaultInput = new();
                // Copy LATEST aim  direction, this is better than nothing atleast
                if (m_inputBuffer.Count() > 0) defaultInput.aimVector = m_inputBuffer.Last().aimVector;

                if (DEBUG) Debug.Log($"[{tick}]EmulatedInput - Interpolation mode: NONE (default value): {defaultInput}");

                return defaultInput;
            }
            else
            {
                if (DEBUG) Debug.Log($"[{tick}]EmulatedInput - Interpolation mode: Full interpolation. From {prevInput} to {nextInput}");
            }

            interpInput = prevInput;

            interpInput.Interpolate(nextInput, tick);

            if (DEBUG) Debug.Log($"Interpolation Result: {interpInput}");

            return interpInput;
        }

        public void StartInputReplay(int timestamp)
        {
            m_inputReplayTick = timestamp;
        }

        public int StopInputReplay()
        {
            var lastReplayTick = m_inputReplayTick - 1;
            m_inputReplayTick = NO_REPLAY;
            return lastReplayTick;
        }
    }
}
