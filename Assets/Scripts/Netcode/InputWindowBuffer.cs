using System;
using Tankito.Utils;
using UnityEngine;
using Unity.Netcode;
using Tankito.Netcode.Messaging;

namespace Tankito.Netcode
{
    // Uso exclusivo CLIENTE
    public class InputWindowBuffer : Singleton<InputWindowBuffer>
    {
        public const int WINDOW_SIZE = 15;

        public FixedSizeQueue<InputPayload> inputWindow = new FixedSizeQueue<InputPayload>(WINDOW_SIZE);
        [SerializeField] private bool DEBUG;

        public void AddInputToWindow(InputPayload newInput)
        {
            inputWindow.Enqueue(newInput);
            if (!NetworkManager.Singleton.IsServer) MessageHandlers.Instance.SendInputWindowToServer(inputWindow);
            if (DEBUG) PrintWindowTicks();
        }

        public void PrintWindowTicks()
        {
            var res = "[ ";
            var arr = inputWindow.ToArray();
            for(int i=0;i<WINDOW_SIZE;i++)
            {
                res += arr[i].timestamp + ((i<WINDOW_SIZE-1) ? " | " : " ]");
            }
            Debug.Log("InputWindowBuffer:" + res);
        }
    }
}
