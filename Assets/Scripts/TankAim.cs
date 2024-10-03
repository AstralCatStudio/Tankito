using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TankAim : MonoBehaviour
{
    [Tooltip("How fast the turret can turn to aim in the specified direction.")]
    [SerializeField]
    private float m_aimSpeed = 720f;
    private Vector2 m_lookVector = Vector2.zero;
    private Vector2 m_lastInputVector = Vector2.zero;

    void OnEnable()
    {
        m_lastInputVector = transform.right;
    }

    void FixedUpdate()
    {
        // [ Mathf.Epsilon probablemente habria que cambiarlo por un grado de deadzone para lidiar con el stick drift ]
        // [ o a lo mejor no, porque el propio input system implementa procesadores para el deadzone, etc. Hay que verlo ]
        
        // Actualizar direcc. de apuntado solo si el joystick esta siendo desplazado
        m_lastInputVector = (m_lookVector.magnitude > Mathf.Epsilon) ? m_lookVector : m_lastInputVector;

        var targetAngle = Vector2.SignedAngle(transform.right, m_lastInputVector);
        float rotDeg = 0f;

        if(Mathf.Abs(targetAngle) >= Time.fixedDeltaTime*m_aimSpeed)
        {
            rotDeg = Mathf.Sign(targetAngle)*Time.fixedDeltaTime*m_aimSpeed;
        }
        else
        {
            rotDeg = targetAngle;
        }

        if (Mathf.Abs(rotDeg) > Mathf.Epsilon)
        {
            transform.rotation = Quaternion.Euler(0,0,transform.eulerAngles.z+rotDeg);
        }
    }

    public void OnLook(InputAction.CallbackContext ctx)
    {
        var input = ctx.ReadValue<Vector2>();

        if (ctx.control.path != "/Mouse/position")
        {
            m_lookVector = new Vector2(input.x, input.y);
        }
        else
        {
            // Mouse control fallback/input processing
            m_lookVector = input - (Vector2)Camera.main.WorldToScreenPoint(transform.position);
        }

        if (m_lookVector.magnitude > 1)
        {
            m_lookVector.Normalize();
        }

        //Debug.Log($"OnLook m_lookVector: {m_lookVector}");
    }
}
