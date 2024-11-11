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
        private SortedSet<InputPayload> inputBuffer = new SortedSet<InputPayload>(new ByTimestamp());
        private InputPayload replayInput;

        private const int N_IDEAL_INPUT = 15;
        public int IdealBufferSize { get => N_IDEAL_INPUT; }
        public int BufferSize { get => inputBuffer.Count; }

        public void AddInput(InputPayload[] newInputWindow)
        {
            if (newInputWindow[0].timestamp < SimClock.TickCounter) // Stale Input, don't add
            {
                return;
            }

            inputBuffer.UnionWith(newInputWindow);

            RemoveStaleInput();
        }

        public InputPayload GetInput()
        {
            RemoveStaleInput();

            return PopInput();
        }
        
        public void RemoveStaleInput()
        {
            while( inputBuffer.Count > 0 && inputBuffer.First() < SimClock.TickCounter)
            {
                inputBuffer.Remove(inputBuffer.First());
            }
        }

        private InputPayload PopInput()
        {
            InputPayload? input = inputBuffer.First();

            if (input != null)
            {
                inputBuffer.Remove(inputBuffer.First());
                replayInput = (InputPayload)input;
            }
            
            return replayInput;
        }
        


        public void StartInputReplay(int timestamp) { }
        public int StopInputReplay() { return -1; }
    }

}