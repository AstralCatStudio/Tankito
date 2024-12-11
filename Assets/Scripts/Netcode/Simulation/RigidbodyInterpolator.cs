using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyInterpolator : MonoBehaviour
{
    [SerializeField] Rigidbody2D m_targetRigidbody;
    [SerializeField] float m_interpolationTime;
    [SerializeField] float m_interpolationSpeed;
}
