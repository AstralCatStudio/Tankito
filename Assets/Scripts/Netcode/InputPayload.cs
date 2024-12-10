using System;
using System.Collections.Generic;
using Unity.Mathematics;
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

    public class ByTimestamp : IComparer<InputPayload>
    {
        public int Compare(InputPayload x, InputPayload y)
        {
            return x.timestamp-y.timestamp;
        }
    }

    public struct InputPayload : INetworkSerializable, IComparable<int>
    {
        public int timestamp; // net clock count
        public Vector2 moveVector;
        public Vector2 aimVector;
        public TankAction action;

        public int CompareTo(int other)
        {
            return timestamp-other;
        }

        //public static readonly InputPayload INVALID_INPUT = new InputPayload { timestamp = -1, moveVector = Vector2.zero, aimVector = Vector2.zero, action = TankAction.None };

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref timestamp);
            serializer.SerializeValue(ref moveVector);
            serializer.SerializeValue(ref aimVector);
            serializer.SerializeValue(ref action);
        }
        
        public override string ToString() => $"Input[{timestamp}](movementInput:{moveVector}, aimInput:{aimVector}, actionInput:{action})";

        internal void Interpolate(InputPayload targetInput, int interpolationTick)
        {
            float factor = Mathf.Clamp01((interpolationTick - timestamp)/(float)(targetInput.timestamp - timestamp));
            Debug.Log("interpolate factor: " + factor + "/1");
            
            moveVector = Vector2.Lerp(moveVector, targetInput.moveVector, factor);
            aimVector = Vector2.Lerp(aimVector, targetInput.aimVector, factor);
            action = (factor < 0.5) ? action : targetInput.action;
        }

        public static bool operator <(InputPayload left, int right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >(InputPayload left, int right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <=(InputPayload left, int right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >=(InputPayload left, int right)
        {
            return left.CompareTo(right) >= 0;
        }
    }
}