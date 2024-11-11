using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class AlgaeAnimation : MonoBehaviour
{
    [SerializeField] private SplineContainer container;
    [SerializeField] private Transform head;
    [SerializeField] private Transform tail;
    [SerializeField] private float maxWidthOffset = 1f;
    [SerializeField] private float speed = 1f;
    [SerializeField] private float random;
    private Quaternion defaultHeadRot;
    private Quaternion defaultTailRot;
    // Start is called before the first frame update
    void Awake()
    {
        container = GetComponent<SplineContainer>();
        random = Random.Range(0, Mathf.PI);
        defaultHeadRot = head.localRotation;
        defaultTailRot = tail.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        Spline spline = container.Spline;
        int knotCount = spline.Count;
        BezierKnot knot;
        

        for (int i = knotCount-1; i > 0; i--)
        {
            float distanceDamping = (float)i / knotCount;
            knot = spline[i];
            knot.Position.x = Mathf.Sin(i * (Mathf.PI / 2) + Time.time * speed + random) * distanceDamping * maxWidthOffset;
            spline[i] = knot;
        }
        knot = spline[0];
        Quaternion knotRotation = new(
            knot.Rotation.value.x,
            knot.Rotation.value.y,
            knot.Rotation.value.z,
            knot.Rotation.value.w
        );

        tail.transform.localRotation = Quaternion.Euler(knotRotation.eulerAngles + defaultTailRot.eulerAngles);
        tail.transform.localPosition = new Vector3(knot.Position.x, knot.Position.y, knot.Position.z);

        knot = container.Spline[^1];    //^1 quiere decir que toma el valor 1 desde el final
        knotRotation = new(
            knot.Rotation.value.x,
            knot.Rotation.value.y,
            knot.Rotation.value.z,
            knot.Rotation.value.w
        );

        head.transform.localRotation = Quaternion.Euler(knotRotation.eulerAngles - defaultHeadRot.eulerAngles);
        head.transform.localPosition = new Vector3(knot.Position.x, knot.Position.y, knot.Position.z);
    }
}
