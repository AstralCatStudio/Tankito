using System;
using Tankito.Utils;
using Unity.Netcode;
using UnityEngine;

namespace Tankito.Netcode
{
    // Uso exclusivo CLIENTE
    public class InputWindowBuffer : Singleton<InputWindowBuffer>
    {
        private const int WINDOW_SIZE = 15;

        private CircularBuffer<InputPayload> inputBuffer = new CircularBuffer<InputPayload>(WINDOW_SIZE);

        public void AddPayloadToWindow(InputPayload newInput)
        {
            inputBuffer.Add(newInput, newInput.timestamp);
            SendInputWindowServerRPC();
        }
        
        [ServerRpc(Delivery = RpcDelivery.Unreliable)]
        private void SendInputWindowServerRPC()
        {
            // We should bit-pack the shit out of this, but it's ok for now.
            //throw new NotImplementedException();
        }
    }
}
