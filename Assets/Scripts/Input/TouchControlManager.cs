using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Users;
using UnityEngine.UI;

namespace Tankito.Mobile
{
    public class TouchControlManager : MonoBehaviour
    {
        [SerializeField] Canvas m_canvas;
        private static PointerEventData m_pointerEvtData = new (null);
        private static List<RaycastResult> m_raycastResults = new List<RaycastResult>();
        private GraphicRaycaster m_graphicRaycaster;

        
        void OnEnable()
        {
            EnhancedTouchSupport.Enable();
            InputUser.onChange += OnInputDeviceChange;
        }

        void OnDisable()
        {
            EnhancedTouchSupport.Disable();
            InputUser.onChange -= OnInputDeviceChange;
        }
        
        private void OnInputDeviceChange(InputUser user, InputUserChange change, InputDevice device)
        {
            //Debug.Log($"User:{user}, change:{change}, device:{device}");
            if (device == null) return;
            if (device.ToString().Contains("Mobile") || device.ToString().Contains("Gamepad") || device.ToString().Contains("Touch") &&
            (change == InputUserChange.DevicePaired || change == InputUserChange.DeviceRegained))
            {
                m_canvas.gameObject.SetActive(true);
            } else {
                m_canvas.gameObject.SetActive(false);
            }
        }

        public bool CheckIfTouched(Vector2 absoluteScreenPosition, Type componentType)
        {
            bool res = false;

            m_pointerEvtData.position = absoluteScreenPosition;
            m_raycastResults.Clear();

            // Perform a raycast to find the UI element touched by the finger
            m_graphicRaycaster.Raycast(m_pointerEvtData, m_raycastResults);

            foreach (var hit in m_raycastResults)
            {
                var hasComponent = hit.gameObject.GetComponent(componentType) != null;
                res = (res || hasComponent);
                //Debug.Log($"Checking for {componentType.Name} collisions against {hit.gameObject}, result: {hasComponent}");
            }
            
            //Debug.Log($"Hit {m_raycastResults.Count} elements while checking for {componentType}, result: {res}");

            return res;
        }
        
        void Awake()
        {
            if (m_canvas == null)
            {
                m_canvas = GetComponentInChildren<Canvas>();
                if (m_canvas == null) Debug.LogWarning($" No Canvas component attached to {this}!");
            }
            if (m_graphicRaycaster == null)
            {
                m_graphicRaycaster = m_canvas.GetComponent<GraphicRaycaster>();
                if (m_graphicRaycaster == null) Debug.LogWarning($"No GraphicRaycaster component attached to {this}, some touch controls might not function properly...");
            }
        }


    }
}

