using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;

namespace Tankito.Mobile
{
    public class TouchControlManager : MonoBehaviour
    {
        [SerializeField] Canvas m_canvas;
        private static PointerEventData m_pointerEvtData = new (null);
        private static List<RaycastResult> m_raycastResults = new List<RaycastResult>();
        public GraphicRaycaster graphicRaycaster;
        void OnEnable()
        {
            EnhancedTouchSupport.Enable();
        }

        void OnDisable()
        {
            EnhancedTouchSupport.Disable();
        }

        public bool CheckIfTouched(Vector2 absoluteScreenPosition, Type componentType)
        {
            bool res = false;

            m_pointerEvtData.position = absoluteScreenPosition * 1f/m_canvas.scaleFactor;
            m_raycastResults.Clear();

            // Perform a raycast to find the UI element touched by the finger
            graphicRaycaster.Raycast(m_pointerEvtData, m_raycastResults);
            foreach (var hit in m_raycastResults)
            {
                var hasComponent = hit.gameObject.GetComponent(componentType) != null;
                Debug.Log($" {hit} is {componentType} ? {hasComponent}");
                res = (res || hasComponent);
            }
            
            Debug.Log($"Checking for {componentType.Name} collisions, result: {res}");

            return res;
        }
        
        void Awake()
        {
            if (graphicRaycaster == null)
            {
                graphicRaycaster = GetComponent<GraphicRaycaster>();
                Debug.LogWarning($"No GraphicRaycaster component attached to {this}, some touch controls might not function properly...");
            }
            if (m_canvas == null)
            {
                Canvas[] c = GetComponentsInParent<Canvas>();
                m_canvas = c[c.Length - 1];
                if (m_canvas == null) Debug.LogWarning($" No Canvas component attached to {this}!");
            }
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        

    }
}

