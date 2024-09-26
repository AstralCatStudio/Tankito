using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;
using ETouch = UnityEngine.InputSystem.EnhancedTouch;

namespace Tankito.Mobile
{
    public enum ScreenHotspot
    {
        Left = 0,
        Right = 1
    }

    public class TouchJoystick : OnScreenControl
    {
        [SerializeField]
        private Vector2 JoystickSize = new Vector2(300, 300);
        [SerializeField]
        private FloatingJoystick Joystick;
        
        [InputControl(layout = "Vector2")]
        [SerializeField] private string m_ControlPath;
        protected override string controlPathInternal
        {
            get => m_ControlPath;
            set => m_ControlPath = value;
        }

        private Finger JoystickFinger;
        private Vector2 DisplacementAmount;
        [SerializeField] private ScreenHotspot m_hotspotPosition;
        [SerializeField] private float m_hotspotDeadzone = 100f;



        protected override void OnEnable()
        {
            base.OnEnable();

            EnhancedTouchSupport.Enable();
            ETouch.Touch.onFingerDown += HandleFingerDown;
            ETouch.Touch.onFingerUp += HandleLoseFinger;
            ETouch.Touch.onFingerMove += HandleFingerMove;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            ETouch.Touch.onFingerDown -= HandleFingerDown;
            ETouch.Touch.onFingerUp -= HandleLoseFinger;
            ETouch.Touch.onFingerMove -= HandleFingerMove;
            EnhancedTouchSupport.Disable();
        }

        private void HandleFingerMove(Finger MovedFinger)
        {
            if (MovedFinger == JoystickFinger)
            {
                Vector2 knobPosition;
                float maxMovement = JoystickSize.x / 2f;
                ETouch.Touch currentTouch = MovedFinger.currentTouch;

                if (Vector2.Distance(
                        currentTouch.screenPosition,
                        Joystick.RectTransform.anchoredPosition
                    ) > maxMovement)
                {
                    knobPosition = (
                        currentTouch.screenPosition - Joystick.RectTransform.anchoredPosition
                        ).normalized
                        * maxMovement;
                }
                else
                {
                    knobPosition = currentTouch.screenPosition - Joystick.RectTransform.anchoredPosition;
                }

                Joystick.Knob.anchoredPosition = knobPosition;
                DisplacementAmount = knobPosition / maxMovement;
                SendValueToControl(DisplacementAmount);
            }
        }

        private void HandleLoseFinger(Finger LostFinger)
        {
            if (LostFinger == JoystickFinger)
            {
                JoystickFinger = null;
                Joystick.Knob.anchoredPosition = Vector2.zero;
                Joystick.gameObject.SetActive(false);
                DisplacementAmount = Vector2.zero;
            }
            SendValueToControl(Vector2.zero);
        }

        private void HandleFingerDown(Finger TouchedFinger)
        {
            if (JoystickFinger != null) return;

            switch(m_hotspotPosition)
            {
                case ScreenHotspot.Left:
                    if (TouchedFinger.screenPosition.x+Mathf.Abs(m_hotspotDeadzone) >= Screen.width / 2f)
                        return;
                break;

                case ScreenHotspot.Right:
                    if (TouchedFinger.screenPosition.x-Mathf.Abs(m_hotspotDeadzone) <= Screen.width / 2f)
                        return;
                break;
                
                default:
                    Debug.LogWarning("Invalid hotspotPosition configured");
                    return;
                break;
            }
            
            JoystickFinger = TouchedFinger;
            DisplacementAmount = Vector2.zero;
            Joystick.gameObject.SetActive(true);
            Joystick.RectTransform.sizeDelta = JoystickSize;
            Joystick.RectTransform.anchoredPosition = ClampStartPosition(TouchedFinger.screenPosition);
        }

        private Vector2 ClampStartPosition(Vector2 StartPosition)
        {
            if (StartPosition.x < JoystickSize.x / 2)
            {
                StartPosition.x = JoystickSize.x / 2;
            }

            if (StartPosition.y < JoystickSize.y / 2)
            {
                StartPosition.y = JoystickSize.y / 2;
            }
            else if (StartPosition.y > Screen.height - JoystickSize.y / 2)
            {
                StartPosition.y = Screen.height - JoystickSize.y / 2;
            }

            return StartPosition;
        }

        private void OnGUI()
        {
            GUIStyle labelStyle = new GUIStyle()
            {
                fontSize = 24,
                normal = new GUIStyleState()
                {
                    textColor = Color.white
                }
            };
            if (JoystickFinger != null)
            {
                GUI.Label(new Rect(10, 35, 500, 20), $"Finger Start Position: {JoystickFinger.currentTouch.startScreenPosition}", labelStyle);
                GUI.Label(new Rect(10, 65, 500, 20), $"Finger Current Position: {JoystickFinger.currentTouch.screenPosition}", labelStyle);
            }
            else
            {
                GUI.Label(new Rect(10, 35, 500, 20), "No Current Movement Touch", labelStyle);
            }

            GUI.Label(new Rect(10, 10, 500, 20), $"Screen Size ({Screen.width}, {Screen.height})", labelStyle);
        }

        /* #if UNITY_EDITOR
            [CustomEditor(typeof(PlayerTouchMovement))]
            internal class PlayerTouchMovementEditor : UnityEditor.Editor
            {
                private SerializedProperty m_ControlPathInternal;
                public void OnEnable()
                {
                    m_ControlPathInternal = serializedObject.FindProperty(nameof(PlayerTouchMovement.m_ControlPath));
                }
                public override void OnInspectorGUI()
                {
                    EditorGUILayout.PropertyField(m_ControlPathInternal);

                    serializedObject.ApplyModifiedProperties();
                }
            }
        #endif */
    }

}
