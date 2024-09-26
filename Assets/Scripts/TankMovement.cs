using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TankMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody2D m_tankRB;
    [SerializeField] private float m_speed = 25.0f;
    [SerializeField] private float m_rotationSpeed = 1.0f;
    private Vector2 m_movementVector = Vector2.zero;

    // Start is called before the first frame update
    void Start()
    {
        if (m_tankRB == null)
        {
            m_tankRB = GetComponent<Rigidbody2D>();
            if (m_tankRB == null)
            {
                Debug.Log("Error tank Rigibody2D reference not set.");
            }
        }
    }

    void FixedUpdate()
    {
        if (m_movementVector.magnitude <= Mathf.Epsilon) return;
        
        var targetAngle = Vector2.SignedAngle(transform.right, m_movementVector);
        var rotDir = Mathf.Sign(targetAngle);

        if (Mathf.Abs(targetAngle) >= Time.fixedDeltaTime*m_rotationSpeed)
        {
            var rotDeg = rotDir*Time.fixedDeltaTime*m_rotationSpeed;
            
            m_tankRB.MoveRotation(m_tankRB.rotation+rotDeg);
        }

        m_tankRB.MovePosition(m_tankRB.position + m_movementVector*Time.fixedDeltaTime*m_speed);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        var input = context.ReadValue<Vector2>();
        m_movementVector = new Vector2(input.x, input.y);
    }
}

