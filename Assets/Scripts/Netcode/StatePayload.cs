using Unity.Netcode;
using UnityEngine;

namespace Tankito.Netcode
{
    public struct StatePayload : INetworkSerializable
    {
        public int timestamp;
        public Vector2 position;
        public float rotation;
        public Vector2 velocity;
       //public static readonly StatePayload INVALID_STATE = new StatePayload { timestamp = -1, position = Vector2.zero, rotation = 0, velocity = Vector2.zero };

        // USAR DELTAS ?? si porfi quiero usar deltas pero hay que investigar como
        // INCLUIR VARIABLES DE SIMULACION RELEVANTES (probablemente solo velocidad lineal y angular)

        public TankAction performedAction; // Probablemente relevante incluirlo en el estado,
        // tambien hay que pensar si queremos y como incluir sincronizacion de animaciones,
        // a lo mejor basta con usar algo preexistente ya sea de Unity o de terceros...
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref timestamp);
            serializer.SerializeValue(ref position);
            serializer.SerializeValue(ref rotation);
            serializer.SerializeValue(ref performedAction);
        }
        public override string ToString() => $"(timestamp:{timestamp}, postition:{position}, rotation:{rotation}, actionInput:{performedAction})";
    }

}