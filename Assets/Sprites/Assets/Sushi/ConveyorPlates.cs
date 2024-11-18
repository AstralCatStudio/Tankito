using UnityEngine;

public class ConveyorPlates : MonoBehaviour
{
    public enum Direction { Left, Right }
    public Direction movementDirection = Direction.Right;
    public float speed = 2f;
    public float offset = 0.5f;

    private float screenLeftEdge;
    private float screenRightEdge;
    private float plateWidth; // Ancho del plano

    void Start()
    {
        // Calcula los bordes de la pantalla
        screenLeftEdge = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).x;
        screenRightEdge = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, 0)).x;

        // Calcula el ancho del plano basado en el tamaño del SpriteRenderer o del objeto
        var renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            plateWidth = renderer.bounds.size.x;
        }
        else
        {
            plateWidth = transform.localScale.x; // Alternativa para otros tipos de objetos
        }
    }

    void Update()
    {
        float move = speed * Time.deltaTime;
        if (movementDirection == Direction.Right)
        {
            transform.Translate(Vector3.right * move);

            // Reposicionar el plano si sale del borde derecho
            if (transform.position.x >= screenRightEdge + plateWidth / 2)
            {
                transform.position = new Vector3(screenLeftEdge - plateWidth / 2, transform.position.y, transform.position.z);
            }
        }
        else if (movementDirection == Direction.Left)
        {
            transform.Translate(Vector3.left * move);

            // Reposicionar el plano si sale del borde izquierdo
            if (transform.position.x <= screenLeftEdge - plateWidth / 2)
            {
                transform.position = new Vector3(screenRightEdge + plateWidth / 2, transform.position.y, transform.position.z);
            }
        }
    }
}
