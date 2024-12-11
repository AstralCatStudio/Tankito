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

using ETouch = UnityEngine.InputSystem.EnhancedTouch;

namespace Tankito.Mobile
{
    public class TouchControlManager : MonoBehaviour
    {
        [SerializeField]
        Canvas m_canvas;
        private static PointerEventData m_pointerEvtData = new (null);
        private static List<RaycastResult> m_raycastResults = new List<RaycastResult>();
        private GraphicRaycaster m_graphicRaycaster;

        private bool m_delayedGUIRemoval = false;
        private bool m_forcedHide = false;

        
        void OnEnable()
        {
            EnhancedTouchSupport.Enable();
            
            UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown += (Finger f) => ActivateTouchGUI();
            InputUser.onChange += OnInputDeviceChange;
        }

        void OnDisable()
        {
            EnhancedTouchSupport.Disable();
            
            InputUser.onChange -= OnInputDeviceChange;
            UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown -= (Finger f) => ActivateTouchGUI();
        }

        public void ForceHideTouchGUI()
        {
            m_canvas.gameObject.SetActive(false);
            m_forcedHide = true;
        }

        public void ReleaseForceHideTouchGUI()
        {
            m_forcedHide = false;
        }

        private void ActivateTouchGUI()
        {
            Debug.Log("ActivateTouchGUI() called.");
            if (m_forcedHide) return;
            
            if (ETouch.Touch.activeTouches.Count > 0)
            {
                m_canvas.gameObject.SetActive(true);
            }
        }

        private void DeactivateTouchGUI()
        {
            Debug.Log($"DeactivateTouchGUI({m_delayedGUIRemoval}) called.");
            if (ETouch.Touch.activeTouches.Count == 0)
            {
                m_canvas.gameObject.SetActive(false);
            }
            else if (!m_delayedGUIRemoval)
            {
                ETouch.Touch.onFingerUp += (Finger f) => DeactivateTouchGUI();
            }
            else if (ETouch.Touch.activeTouches.Count == 1) // && delayedGUIRemoval) // Se comprueba con un toque en lugar de 0 porque onFingerUp se llama justo antes de liberar el ultimo toque activo.
            {
                ETouch.Touch.onFingerUp -= (Finger f) => DeactivateTouchGUI();
                Debug.Log($"Deactivated TouchGUI with delayed GUI Removal! (dealyedRemoval check: {m_delayedGUIRemoval})");
                m_canvas.gameObject.SetActive(false);
            }
        }
        
        private void OnInputDeviceChange(InputUser user, InputUserChange change, InputDevice device)
        {
            if (device == null) return;
            
            if (device is Gamepad)
            {
                switch(change)
                {
                    case InputUserChange.DevicePaired:
                        ActivateTouchGUI();
                    break;

                    case InputUserChange.DeviceUnpaired:
                        DeactivateTouchGUI();
                    break;
                }
            }
            

            //Debug.Log($"User:{user} | change:{change} | device:{device}");
        }

        public Component CheckTouch<T>(Vector2 absoluteScreenPosition)
        {
            m_pointerEvtData.position = absoluteScreenPosition;
            m_raycastResults.Clear();

            // Perform a raycast to find the UI element touched by the finger
            m_graphicRaycaster.Raycast(m_pointerEvtData, m_raycastResults);

            foreach (var hit in m_raycastResults)
            {
                var hitComponent = hit.gameObject.GetComponent(typeof(T));
                var hasComponent = hit.gameObject.GetComponent(typeof(T)) != null;
                if (hasComponent) return hitComponent;
                //Debug.Log($"Checking for {componentType.Name} collisions against {hit.gameObject}, result: {hasComponent}");
            }

            //Debug.Log($"No {typeof(T)} found in query {absoluteScreenPosition}");
            return default;
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

