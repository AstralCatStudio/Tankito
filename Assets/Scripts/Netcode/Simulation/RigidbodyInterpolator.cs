using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyInterpolator : MonoBehaviour
{
    [SerializeField] Rigidbody2D m_targetRigidbody;
    [SerializeField] float m_interpolationSpeed = 1;

    void Update()
    {
        float t = m_interpolationSpeed/10f;
        transform.position = Vector3.Lerp(transform.position, m_targetRigidbody.position, t);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.AngleAxis(m_targetRigidbody.rotation, Vector3.forward), t);
    }
}
