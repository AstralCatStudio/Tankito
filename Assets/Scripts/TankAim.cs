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
        if (m_lookVector.magnitude > Mathf.Epsilon)
        {
            m_lastInputVector = m_lookVector;
        }
        else return;

        var targetAngle = Vector2.SignedAngle(transform.right, m_lastInputVector);

        var rotDir = Mathf.Sign(targetAngle);
        var rotDeg = rotDir*Time.fixedDeltaTime*m_aimSpeed;

        transform.rotation = Quaternion.Euler(0,0,transform.eulerAngles.z+rotDeg);
    }

    public void OnLook(InputAction.CallbackContext ctx)
    {
        var input = ctx.ReadValue<Vector2>();
        m_lookVector = new Vector2(input.x, input.y);
    }
}
