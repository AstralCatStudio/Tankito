using Unity.Netcode;
using UnityEngine;

namespace Tankito.Netcode
{
    public struct StatePayload
    {
        public int timestamp;
        public Vector2 position;
        public float rotation;

        // USAR DELTAS ?? si porfi quiero usar deltas pero hay que investigar como
        // INCLUIR VARIABLES DE SIMULACION RELEVANTES (probablemente solo velocidad lineal y angular)

        public TankAction actionInput; // Probablemente relevante incluirlo en el estado,
        // tambien hay que pensar si queremos y como incluir sincronizacion de animaciones,
        // a lo mejor basta con usar algo preexistente ya sea de Unity o de terceros...
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref timestamp);
            serializer.SerializeValue(ref position);
            serializer.SerializeValue(ref rotation);
            serializer.SerializeValue(ref actionInput);
        }
        public override string ToString() => $"(timestamp:{timestamp}, psotiion:{position}, rotation:{rotation}, actionInput:{actionInput})";
    }
}