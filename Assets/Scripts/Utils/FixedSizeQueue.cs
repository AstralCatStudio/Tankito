using System.Collections.Generic;
using UnityEngine;

namespace Tankito.Utils
{
    public class FixedSizeQueue<T> : Queue<T>
    {
        private int m_size;

        public int Capacity { get => m_size; }

        public FixedSizeQueue(int size) : base(size)
        {
            m_size = size;
        }

        public new void Enqueue(T value)
        {
            base.Enqueue(value);
            
            while (Count > Capacity)
            {
                Dequeue();
            }
        }

        public override string ToString()
        {
            var res = "[ ";
            var arr = ToArray();
            for(int i=0; i<Capacity;i++)
            {
                res += ((i < arr.Length) ? arr[i] : "___") + ((i < Capacity-1) ? " | " : " ]");
            }
            return res;
        }
    }
}