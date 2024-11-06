using System;

namespace Tankito.Utils
{
    public static class Math
    {
        /// <summary>
        /// Computes the modulus operation of <paramref name="x"/> into <paramref name="m"/>.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="m">  presumed to always be a positive integer </param>
        public static int Mod(int x, int m)
        {
            return (x%m + m)%m;
        }
    }
    
    public class CircularBuffer<T>
    {
        private T[] buffer;
        private int size;
        
        public CircularBuffer(int capacity)
        {
            buffer = new T[capacity];
            size = capacity;
        }

        /// <summary>
        /// Adds an item to the circular buffer at a specific index (clamped to buffer size).
        /// </summary>
        /// <param name="item"></param>
        /// <param name="i"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void Add(T item, int i)
        {
            int idx = Math.Mod(i,size);

            if (idx < 0 || idx >= size)
            {
                throw new ArgumentOutOfRangeException("Index is out of the bounds of the buffer.");
            }
            buffer[idx] = item;
        }

        /// <summary>
        /// Gets an item from the circular buffer at a specific index (clamped to buffer size).
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public T Get(int i)
        {
            int idx = Math.Mod(i,size);

            if (idx < 0 || idx >= size)
            {
                throw new ArgumentOutOfRangeException("Index is out of the bounds of the buffer.");
            }
            return buffer[idx];
        }

        /// <summary>
        /// Clears the circular buffer.
        /// </summary>
        public void Clear()
        {
            Array.Clear(buffer, 0, size);
        }
    }
}