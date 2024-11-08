using System;
using Tankito.Utils;
using Unity.Netcode;
using Tankito.Netcode.Messaging;

namespace Tankito.Netcode
{
    // Uso exclusivo CLIENTE
    public class InputWindowBuffer : Singleton<InputWindowBuffer>
    {
        public const int    WINDOW_SIZE = 15;

        private CircularBuffer<InputPayload> inputBuffer = new CircularBuffer<InputPayload>(WINDOW_SIZE);

        public CircularBuffer<InputPayload> InputBuffer { get => inputBuffer; }

        public void AddInputToWindow(InputPayload newInput)
        {
            inputBuffer.Add(newInput, newInput.timestamp);
            CustomNamedMessageHandler.Instance.SendInputWindowToServer(inputBuffer);
        }
    }
}
