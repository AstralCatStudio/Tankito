using Unity.Netcode;
using UnityEngine;

namespace Tankito.Netcode
{
    public enum TankAction
    {
        None = 0,
        Dash = 1,
        Parry = 2
    }
    public struct InputPayload
    {
        public int timestamp;
        public Vector2 movementInput;
        public Vector2 aimInput;
        public TankAction actionInput;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref timestamp);
            serializer.SerializeValue(ref movementInput);
            serializer.SerializeValue(ref aimInput);
            serializer.SerializeValue(ref actionInput);
        }
        public override string ToString() => $"(timestamp:{timestamp}, movementInput:{movementInput}, aimInput:{aimInput}, actionInput:{actionInput})";
    }
}