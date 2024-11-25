using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tankito.Utils;
using Unity.Services.Matchmaker.Models;
using UnityEngine;

namespace Tankito.Netcode.Simulation
{
    public class RemoteTankInput : MonoBehaviour, ITankInput
    {
        private SortedSet<InputPayload> m_inputBuffer = new SortedSet<InputPayload>(new ByTimestamp());
        private InputPayload m_replayInput;
        [SerializeField] private bool DEBUG;

        //const int N_IDEAL_INPUT = 10;
        public int IdealBufferSize { get => SimulationParameters.SERVER_IDEAL_INPUT_BUFFER_SIZE; }
        public int Last { get =>  (m_inputBuffer.Count != 0) ? m_inputBuffer.Last().timestamp : SimClock.TickCounter; }

        public void AddInput(InputPayload[] newInputWindow)
        {
            // Add only new ticks' inputs.
            m_inputBuffer.UnionWith(newInputWindow.Where(i => i.timestamp > SimClock.TickCounter));

            RemoveStaleInput();

            if (DEBUG)
            {
                var debug = $"[{SimClock.TickCounter}]Remote Input Window: [ ";
                foreach(var i in m_inputBuffer)
                {
                    if (i.timestamp == SimClock.TickCounter)
                        debug += i.timestamp + " | ";
                    else if (i.timestamp == (SimClock.TickCounter + IdealBufferSize))
                        debug += i.timestamp + " | ";
                    else if (!i.Equals(m_inputBuffer.Last()))
                        debug += i.timestamp + ", ";
                    else
                        debug += i.timestamp + " ]";
                }
                Debug.Log(debug);
            }
        }

        public InputPayload GetInput()
        {
            RemoveStaleInput();

            return PopInput();
        }
        
        public void RemoveStaleInput()
        {
            while( m_inputBuffer.Count > 0 && m_inputBuffer.First() < SimClock.TickCounter)
            {
                m_inputBuffer.Remove(m_inputBuffer.First());
            }
        }

        private InputPayload PopInput()
        {
            InputPayload input = m_inputBuffer.FirstOrDefault();

            if (!input.Equals(default(InputPayload)))
            {
                m_inputBuffer.Remove(m_inputBuffer.First());
                m_replayInput = input;
            }            
            return m_replayInput;
        }
        
        public InputPayload GetCurrentInput()
        {
            return m_replayInput;
        }

        public void StartInputReplay(int timestamp) { }
        public int StopInputReplay() { return -1; }
    }

}