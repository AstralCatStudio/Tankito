using System;
using System.Collections;
using System.Collections.Generic;

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

        public static int Min(int a, int b)
        {
            return (a < b) ? a : b;
        }
    }
    
    public class CircularBuffer<T> : IEnumerable<T>
    {
        private T[] buffer;
        private int size;
        private int count;
        
        public CircularBuffer(int capacity)
        {
            buffer = new T[capacity];
            size = capacity;
            count = 0;
        }

        /// <summary>
        /// Gets the number of items in the circular buffer.
        /// </summary>
        public int Count => count;

        /// <summary>
        /// Gets the capacity of the circular buffer.
        /// </summary>
        public int Capacity => size;

        /// <summary>
        /// Adds an item to the circular buffer at a specific index (clamped to buffer size).
        /// </summary>
        /// <param name="item"></param>
        /// <param name="i"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void Add(T item, int i)
        {
            int idx = Math.Mod(i, size);

            if (idx < 0 || idx >= size)
            {
                throw new ArgumentOutOfRangeException("Index is out of the bounds of the buffer.");
            }

            buffer[idx] = item;
            count = Math.Min(count + 1, size); // Increment count up to the buffer size
        }

        /// <summary>
        /// Gets an item from the circular buffer at a specific index (clamped to buffer size).
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public T Get(int i)
        {
            int idx = Math.Mod(i, size);

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
            count = 0;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the buffer.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < count; i++)
            {
                yield return buffer[i];
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the buffer.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}