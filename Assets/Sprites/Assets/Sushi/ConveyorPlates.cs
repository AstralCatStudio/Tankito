using UnityEngine;

public class ConveyorPlates : MonoBehaviour
{
    public enum Direction { Left, Right }
    public Direction movementDirection = Direction.Right;
    public float speed = 2f;
    public float offset = 0.5f;

    private float screenLeftEdge;
    private float screenRightEdge;

    void Start()
    {
        // Los bordes
        screenLeftEdge = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).x;
        screenRightEdge = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, 0)).x;
    }

    void Update()
    {
        float move = speed * Time.deltaTime;
        if (movementDirection == Direction.Right)
        {
            transform.Translate(Vector3.right * move);

            if (transform.position.x >= screenRightEdge + offset)
            {
                transform.position = new Vector3(screenLeftEdge - offset, transform.position.y, transform.position.z);
            }
        }
        else if (movementDirection == Direction.Left)
        {
            transform.Translate(Vector3.left * move);

            if (transform.position.x <= screenLeftEdge - offset)
            {
                transform.position = new Vector3(screenRightEdge + offset, transform.position.y, transform.position.z);
            }
        }
    }
}
