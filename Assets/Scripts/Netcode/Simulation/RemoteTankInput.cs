using System.Collections;
using System.Collections.Generic;
using Tankito;
using Tankito.Netcode;
using Tankito.Utils;
using UnityEngine;

public class RemoteTankInput : MonoBehaviour, ITankInput
{
    private OrderQueueSyncronize<InputPayload> queue = new OrderQueueSyncronize<InputPayload>(N_IDEAL_INPUT);

    private const int N_IDEAL_INPUT = 15;

    public void AddInput(InputPayload[] newInputWindow)
    {
        foreach(var input in newInputWindow)
        {
            AddInput(input);
        }
    }

    public void AddInput(InputPayload newInput)
    {
        if(!queue.Contains(newInput.timestamp))
        {
            queue.Add(newInput.timestamp, newInput);
        }
    }

    public InputPayload GetInput()
    {
        return queue.Pop();
    }

    public void StartInputReplay(int timestamp) { }
    public int StopInputReplay() { return -1; }
}
