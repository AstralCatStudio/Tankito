using System.Collections;
using System.Collections.Generic;
using Tankito.Netcode;
using Tankito.Utils;
using UnityEngine;

public class WindowPayloadBuffer : Singleton<WindowPayloadBuffer>
{
    private const int WINDOW_SIZE = 50;

    private int n_added_payloads = 0;

    private CircularBuffer<InputPayload> inputBuffer = new CircularBuffer<InputPayload>(WINDOW_SIZE);
    private CircularBuffer<StatePayload> stateBuffer = new CircularBuffer<StatePayload>(WINDOW_SIZE);

    public void AddPayloadToWindow(InputPayload newInput, StatePayload newState)
    {
        inputBuffer.Add(newInput, newInput.timestamp);
        stateBuffer.Add(newState, newState.timestamp);

        n_added_payloads++;

        if (n_added_payloads == WINDOW_SIZE)
        {
            //SendWindowToAssembler(inputBuffer, stateBuffer);
            n_added_payloads = 0;
        }
    }
}
