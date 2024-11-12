using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

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
        /// <param name="idx"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public T Get(int idx, bool wrapAroundIndex = false)
        {
            T res;

            if (!TryGet(out res, idx, wrapAroundIndex))
            {
                throw new ArgumentOutOfRangeException("Index is out of the bounds of the buffer.");
            }

            return res;
        }

        public bool TryGet(out T value, int idx, bool wrapAroundIndex = false)
        {
            int i = (wrapAroundIndex) ? Math.Mod(idx, size) : idx;

            if (i < 0 || i >= size)
            {
                value = default;
                return false;
            }

            value = buffer[i];
            return true;
        }

        public T this[int i]
        {
            get => Get(i);
            set => Add(value, i);
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

        public override string ToString()
        {
            string res = "{";
            for (int i = 0; i < size; i++)
            {
                res += buffer[i] + ((i < size-1) ? ", " : "}");
            }
            return res;
        }
    }

    static class EnumerableExtensions {
        public static T MaxBy<T,U>(this IEnumerable<T> source, Func<T,U> selector) where U : IComparable<U>
        {
            if (source == null) throw new ArgumentNullException("source");

            bool first = true;
            T maxObj = default(T);
            U maxKey = default(U);

            foreach (var item in source)
            {
                if (first)
                {
                    maxObj = item;
                    maxKey = selector(maxObj);
                    first = false;
                }
                else
                {
                    U currentKey = selector(item);
                    if (currentKey.CompareTo(maxKey) > 0)
                    {
                        maxKey = currentKey;
                        maxObj = item;
                    }
                }
            }

            if (first) throw new InvalidOperationException("Sequence is empty.");

            return maxObj;
        }

        public static T MinBy<T,U>(this IEnumerable<T> source, Func<T,U> selector) where U : IComparable<U>
        {
            if (source == null) throw new ArgumentNullException("source");

            bool first = true;
            T minObj = default(T);
            U minKey = default(U);

            foreach (var item in source)
            {
                if (first)
                {
                    minObj = item;
                    minKey = selector(minObj);
                    first = false;
                }
                else
                {
                    U currentKey = selector(item);
                    if (currentKey.CompareTo(minKey) < 0)
                    {
                        minKey = currentKey;
                        minObj = item;
                    }
                }
            }

            if (first) throw new InvalidOperationException("Sequence is empty.");

            return minObj;
        }
    }
}