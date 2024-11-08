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

        public delegate void ChangeInBuffer(int dif);
        public event ChangeInBuffer OnCheckThrottling;

        public OrderQueueSyncronize(int idealElements)
        {
            m_idealElements = idealElements;
        }

        public int N_Elemets => m_nElements;
        public int IdealElements => m_nElements;

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
                if (m_keys[0] == ClockManager.TickCounter)
                {
                    T lastPopped = m_queue[0];
                    m_keys.RemoveAt(0);
                    m_queue.RemoveAt(0);
                    m_nElements--;
                    return lastPopped;
                }
                else
                {
                    return lastPopped;
                }
            }
            else
            {
                return lastPopped;
            }

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

        public void CheckThrottling()
        {
            int dif = m_nElements - m_idealElements;
            OnCheckThrottling?.Invoke(dif);
        }
    }
}