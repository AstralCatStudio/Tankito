using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tankito;
using Tankito.Netcode;
using Tankito.Utils;
using UnityEngine;

namespace Tankito.Netcode
{
    public class RemoteTankInput : MonoBehaviour, ITankInput
    {
        //private OrderQueueSyncronize<InputPayload> queue = new OrderQueueSyncronize<InputPayload>(N_IDEAL_INPUT); // Bro no va
        private SortedSet<InputPayload> m_inputBuffer = new SortedSet<InputPayload>(new ByTimestamp());
        private InputPayload m_replayInput;

        private const int N_IDEAL_INPUT = 15;
        public int IdealBufferSize { get => N_IDEAL_INPUT; }
        public int BufferSize { get => m_inputBuffer.Count; }

        public void AddInput(InputPayload[] newInputWindow)
        {
            if (newInputWindow[0].timestamp < SimClock.TickCounter) // Stale Input, don't add
            {
                return;
            }

            m_inputBuffer.UnionWith(newInputWindow);

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
        


        public void StartInputReplay(int timestamp) { }
        public int StopInputReplay() { return -1; }
    }

}