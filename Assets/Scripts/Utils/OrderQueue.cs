using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tankito;
using UnityEngine;

namespace Tankito
{
    public class OrderQueueSyncronize<T>
    {
        private List<int> m_keys = new List<int>();
        private List<T> m_queue = new List<T>();
        private int m_nElements = 0;
        private int m_idealElements;
        private T lastPopped = default(T);

        public OrderQueueSyncronize(int idealElements)
        {
            m_idealElements = idealElements;
        }

        public int Count => m_nElements;
        public int IdealElements => m_idealElements;

        public void Add(int key, T element)
        {
            m_keys.Add(key);
            m_keys.Sort();

            if(m_keys.IndexOf(key) == m_queue.Count)
            {
                m_queue.Add(element);
                m_nElements++;
                return;
            }
            
            for(int i = m_queue.Count - 1; i == m_keys.IndexOf(key); i++) 
            {
                if(i == m_queue.Count - 1)
                {
                    m_queue.Add(m_queue.ElementAt(i));
                }
                m_queue[i] = m_queue[i-1];
            }
            m_nElements++;

        }

        public T Pop()
        {
            if(m_keys.Count > 0)
            {
                if (m_keys[0] == SimClock.TickCounter)
                {
                    lastPopped = m_queue[0];
                    m_keys.RemoveAt(0);
                    m_queue.RemoveAt(0);
                    m_nElements--;
                    return lastPopped;
                }
            }
            
            return lastPopped;

        }

        public T PeekValue()
        {
            if(m_keys.Count > 0)
            {
                if (m_keys[0] == SimClock.TickCounter)
                {
                    return m_queue[0];
                }
            }
            return lastPopped;
        }

        public int? PeekKey()
        {
            if(m_keys.Count > 0)
            {
                return m_keys[0];
            }
            return null;
        }

        public void Clear()
        {
            m_keys.Clear();
            m_queue.Clear();
        }

        public bool Contains(int key)
        {
            if (m_keys.Contains(key))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}