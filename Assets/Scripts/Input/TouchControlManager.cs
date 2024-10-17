using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Users;
using UnityEngine.UI;

namespace Tankito.Mobile
{
    enum GamepadType { None, Virtual, Physical }
    public class TouchControlManager : MonoBehaviour
    {
        [SerializeField]
        Canvas m_canvas;
        private static PointerEventData m_pointerEvtData = new (null);
        private static List<RaycastResult> m_raycastResults = new List<RaycastResult>();
        private GraphicRaycaster m_graphicRaycaster;
        [SerializeField]
        private GamepadType m_boundGamepad = GamepadType.None;

        
        void OnEnable()
        {
            EnhancedTouchSupport.Enable();
            
            UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown += ActivateOnTouch;
            UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerMove += ActivateOnTouch;
            InputUser.onChange += OnInputDeviceChange;
        }

        void OnDisable()
        {
            EnhancedTouchSupport.Disable();
            InputUser.onChange -= OnInputDeviceChange;
            UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown -= ActivateOnTouch;
            UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerMove -= ActivateOnTouch;
        }

        private void ActivateOnTouch(Finger f)
        {
            //if (m_canvas.enabled) 
            m_canvas.gameObject.SetActive(true);
            m_boundGamepad = GamepadType.Virtual;
        }
        
        private void OnInputDeviceChange(InputUser user, InputUserChange change, InputDevice device)
        {
            if (device == null) return;

            if (device.ToString().Contains("Gamepad") || device.ToString().Contains("Touch") && change == InputUserChange.DeviceUnpaired && m_canvas.enabled && m_boundGamepad == GamepadType.Virtual)
            {
                m_canvas.gameObject.SetActive(false);
            }
            
            if (device.ToString().Contains("Gamepad") && change == InputUserChange.DevicePaired && !m_canvas.enabled)
            {
                m_boundGamepad = GamepadType.Physical;
            }

            if (user.pairedDevices.Where( dev => dev.ToString().Contains("Gamepad")).ToArray().Length == 0)
            {
                m_boundGamepad = GamepadType.None;
            }

            Debug.Log($"User:{user} | change:{change} | device:{device} | m_boundGamepad:{m_boundGamepad}");
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

