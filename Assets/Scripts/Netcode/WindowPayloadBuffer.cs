using System.Collections;
using System.Collections.Generic;
using Tankito.Netcode;
using Tankito.Utils;
using UnityEngine;

public class WindowPayloadBuffer : Singleton<WindowPayloadBuffer>
{
    private const int WINDOW_SIZE = 15;

    private CircularBuffer<InputPayload> inputBuffer = new CircularBuffer<InputPayload>(WINDOW_SIZE);

    public void AddPayloadToWindow(InputPayload newInput)
    {
        inputBuffer.Add(newInput, newInput.timestamp);
    }
}
