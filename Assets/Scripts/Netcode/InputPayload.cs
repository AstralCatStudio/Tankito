using System;
using Unity.Netcode;
using UnityEngine;

namespace Tankito.Netcode
{
    public enum TankAction
    {
        None = 0,
        Dash = 1,
        Parry = 2,
        Fire = 3,
    }

    public struct InputPayload : INetworkSerializable
    {
        public int timestamp; // net clock count
        public Vector2 moveVector;
        public Vector2 aimVector;
        public TankAction action;
        //public static readonly InputPayload INVALID_INPUT = new InputPayload { timestamp = -1, moveVector = Vector2.zero, aimVector = Vector2.zero, action = TankAction.None };

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref timestamp);
            serializer.SerializeValue(ref moveVector);
            serializer.SerializeValue(ref aimVector);
            serializer.SerializeValue(ref action);
        }
        public override string ToString() => $"(timestamp:{timestamp}, movementInput:{moveVector}, aimInput:{aimVector}, actionInput:{action})";

        // Probablemente habria q repensar como hacer esto:
        
        //void Serialize(FastBufferWriter writer)
        //{
        //    if (!writer.TryBeginWrite(GetSerializationSize()))
        //    {
        //        throw new OverflowException("Not enough space in the buffer");
        //    }
        //    writer.WriteValue(timestamp);
        //    writer.WriteValue(moveVector);
        //    writer.WriteValue(aimVector);
        //    writer.WriteValue(action);
        //}
    }
}