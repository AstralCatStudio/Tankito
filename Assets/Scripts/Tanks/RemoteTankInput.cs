using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tankito;
using Tankito.Netcode;
using Tankito.Utils;
using Unity.Services.Matchmaker.Models;
using UnityEngine;

namespace Tankito.Netcode
{
    public class RemoteTankInput : MonoBehaviour, ITankInput
    {
        private SortedSet<InputPayload> m_inputBuffer = new SortedSet<InputPayload>(new ByTimestamp());
        private InputPayload m_replayInput;

        private const int N_IDEAL_INPUT = 10;
        public int IdealBufferSize { get => N_IDEAL_INPUT; }
        public int BufferSize { get => m_inputBuffer.Count; }

        public void AddInput(InputPayload[] newInputWindow)
        {
            // Add only new ticks' inputs.
            m_inputBuffer.UnionWith(newInputWindow.Where(i => i.timestamp > SimClock.TickCounter));

            RemoveStaleInput();
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