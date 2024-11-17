using UnityEngine;

public class BodyGuardScript : MonoBehaviour
{
    // Configuración
    [Header("Configuración del Bodyguard")]
    public Transform jugador; // Referencia al jugador
    public Transform aliado; // Referencia al aliado
    public Transform[] puntosPatrulla; // Puntos para patrullar
    public float rangoDeteccionJugador = 10f; // Rango para detectar al jugador
    public float rangoDeteccionBala = 5f; // Rango para detectar balas cerca de aliados
    public float rangoAtaque = 15f; // Rango de ataque
    public int balasDisponibles = 5; // Cantidad de balas disponibles
    public float velocidadMovimiento = 3f; // Velocidad del NPC
    public float velocidadGiro = 5f; // Velocidad de giro

    [Header("Tags y Layers")]
    public string tagBala = "Bala"; // Tag para identificar balas

    // Variables internas
    private int indicePatrulla = 0;

    // Percepciones

    /// <summary>
    /// Comprueba si hay un jugador dentro del rango de detección.
    /// </summary>
    public bool JugadorDetectado()
    {
        if (jugador == null) return false;
        return Vector3.Distance(transform.position, jugador.position) <= rangoDeteccionJugador;
    }

    /// <summary>
    /// Comprueba si hay una bala disparada por el jugador que pueda impactar a un aliado.
    /// </summary>
    public bool BalaDetectada()
    {
        if (aliado == null) return false;

        // Detectar objetos cercanos
        Collider[] objetosCercanos = Physics.OverlapSphere(aliado.position, rangoDeteccionBala);

        // Comprobar si alguno de los objetos tiene el tag "Bala"
        foreach (Collider obj in objetosCercanos)
        {
            if (obj.CompareTag(tagBala))
            {
                return true; // Bala detectada
            }
        }
        return false; // No hay balas detectadas
    }

    /// <summary>
    /// Comprueba si hay un aliado dentro del rango de visión.
    /// </summary>
    public bool AliadoDetectado()
    {
        if (aliado == null) return false;
        return Vector3.Distance(transform.position, aliado.position) <= rangoDeteccionJugador;
    }

    /// <summary>
    /// Comprueba si el jugador está dentro del rango de ataque.
    /// </summary>
    public bool JugadorRangoAtaque()
    {
        if (jugador == null) return false;
        return Vector3.Distance(transform.position, jugador.position) <= rangoAtaque;
    }

    /// <summary>
    /// Comprueba si el NPC tiene munición disponible.
    /// </summary>
    public bool TieneMunicion()
    {
        return balasDisponibles > 0;
    }

    /// <summary>
    /// Comprueba si el NPC está apuntando al jugador.
    /// </summary>
    public bool ApuntandoAJugador()
    {
        if (jugador == null) return false;
        Vector3 direccion = (jugador.position - transform.position).normalized;
        return Vector3.Dot(transform.forward, direccion) > 0.95f; // True si está alineado
    }

















    // Acciones

    /// <summary>
    /// Idle: El personaje patrulla hasta detectar un jugador.
    /// </summary>
    public void Patrullar()
    {
        if (puntosPatrulla.Length == 0) return;

        Transform puntoActual = puntosPatrulla[indicePatrulla];
        MoverHacia(puntoActual);

        // Cambiar al siguiente punto si ya alcanzó el actual
        if (Vector3.Distance(transform.position, puntoActual.position) < 0.5f)
        {
            indicePatrulla = (indicePatrulla + 1) % puntosPatrulla.Length; // Cambiar al siguiente punto en el ciclo
        }
    }

    /// <summary>
    /// Seguir al jugador: El personaje se mueve hacia el jugador.
    /// </summary>
    public void SeguirJugador()
    {
        if (jugador == null) return;
        MoverHacia(jugador);
    }

    /// <summary>
    /// Proteger al aliado: Interceptar balas dirigidas hacia el aliado.
    /// </summary>
    public void ProtegerAliado()
    {
        if (aliado == null) return;

        Collider[] objetosCercanos = Physics.OverlapSphere(aliado.position, rangoDeteccionBala);

        foreach (Collider obj in objetosCercanos)
        {
            if (obj.CompareTag(tagBala))
            {
                MoverHacia(obj.transform); // Mueve el NPC hacia la bala para interceptarla
                return;
            }
        }
    }

    /// <summary>
    /// Apuntar al jugador: Calcula el ángulo de giro para alinearse con el jugador.
    /// </summary>
    public void ApuntarAlJugador()
    {
        if (jugador == null) return;
        GirarHacia(jugador.position);
    }

    /// <summary>
    /// Disparar: Ataca al jugador si está apuntando correctamente.
    /// </summary>
    public void Disparar()
    {
        if (!ApuntandoAJugador() || balasDisponibles <= 0) return;

        // Lógica para disparar
        Debug.Log("Disparando al jugador");
        balasDisponibles--;
    }



















    // Métodos auxiliares

    /// <summary>
    /// Realiza un movimiento hacia el objetivo.
    /// </summary>
    private void MoverHacia(Transform objetivo)
    {
        if (objetivo == null) return;

        Vector3 direccion = (objetivo.position - transform.position).normalized;
        transform.position += direccion * velocidadMovimiento * Time.deltaTime;
        GirarHacia(objetivo.position);
    }

    /// <summary>
    /// Gira hacia una posición.
    /// </summary>
    private void GirarHacia(Vector3 posicionObjetivo)
    {
        Vector3 direccion = (posicionObjetivo - transform.position).normalized;
        Quaternion rotacionObjetivo = Quaternion.LookRotation(direccion);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotacionObjetivo, velocidadGiro * Time.deltaTime);
    }
}
