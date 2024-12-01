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
        private int WINDOW_SIZE { get => SimulationParameters.CLIENT_INPUT_WINDOW_SIZE; }

        public FixedSizeQueue<InputPayload> inputWindow;// = new FixedSizeQueue<InputPayload>(WINDOW_SIZE);
        [SerializeField] private bool DEBUG;

        void Start()
        {
            inputWindow = new FixedSizeQueue<InputPayload>(WINDOW_SIZE);
        }

        public void AddInputToWindow(InputPayload newInput)
        {
            inputWindow.Enqueue(newInput);
            if (DEBUG) PrintWindowTicks(NetworkManager.Singleton.IsClient && inputWindow.Count == WINDOW_SIZE);
            if (NetworkManager.Singleton.IsClient && inputWindow.Count == WINDOW_SIZE) MessageHandlers.Instance.SendInputWindowToServer(inputWindow);
        }

        public void PrintWindowTicks(bool sent)
        {
            var res = $"(Count={inputWindow.Count}/WindowSize={WINDOW_SIZE})[ ";
            var arr = inputWindow.ToArray();
            for(int i=0;i<WINDOW_SIZE;i++)
            {
                res += ((i < arr.Length) ? arr[i].timestamp : "___") + ((i<WINDOW_SIZE-1) ? " | " : " ]");
            }
            Debug.Log($"[{SimClock.TickCounter}]InputWindowBuffer({(sent ? "SENT" : "NOT-SENT")}):" + res);
        }
    }
}
