using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourAPI.Core;
using BehaviourAPI.UnityToolkit;

[SelectionGroup("BodyGuardActions")]
public class SeguirJugadorAction : UnityAction
{
    private Transform jugador; // Referencia al jugador
    private float velocidad = 3f; // Velocidad de movimiento
    private float distanciaMinima = 2f; // Distancia m�nima que el NPC debe mantener

    protected override void OnSetContext()
    {
        GameObject jugadorObj = GameObject.FindWithTag("Player");
        if (jugadorObj != null)
        {
            jugador = jugadorObj.transform;
            Debug.Log("SeguirJugadorAction: Jugador encontrado correctamente.");
        }
        else
        {
            Debug.LogWarning("SeguirJugadorAction: No se encontr� un GameObject con el tag 'Player'.");
        }
    }

    public override void Start()
    {
        if (jugador == null)
        {
            Debug.LogError("SeguirJugadorAction: No se puede ejecutar la acci�n porque el jugador es null.");
        }
        else
        {
            Debug.Log("SeguirJugadorAction: Iniciando acci�n de seguimiento al jugador.");
        }
    }

    public override Status Update()
    {
        if (jugador == null)
        {
            Debug.LogWarning("SeguirJugadorAction: Jugador no est� definido. Terminando acci�n.");
            return Status.Failure;
        }

        // Calcula la distancia al jugador
        float distancia = Vector2.Distance(context.Transform.position, jugador.position);

        // Si est� dentro de la distancia m�nima, no moverse
        if (distancia <= distanciaMinima)
        {
            Debug.Log($"SeguirJugadorAction: Distancia m�nima alcanzada ({distancia} <= {distanciaMinima}). Deteni�ndose.");
            return Status.Running; // Mantener el estado sin moverse
        }

        // Mueve hacia el jugador
        MoverHacia(jugador);

        // Gira hacia el jugador
        GirarHacia(jugador.position);

        return Status.Running; // La acci�n sigue ejecut�ndose
    }

    public override void Stop()
    {
        Debug.Log("SeguirJugadorAction: Acci�n detenida.");
    }

    // M�todos Auxiliares

    private void MoverHacia(Transform objetivo)
    {
        if (objetivo == null) return;

        // Calcula la direcci�n en 2D (X-Y)
        Vector2 direccion = (objetivo.position - context.Transform.position);
        direccion.Normalize();

        // Mueve el NPC en el plano X-Y
        Vector2 nuevaPosicion = (Vector2)context.Transform.position + direccion * velocidad * Time.deltaTime;
        context.Transform.position = new Vector3(nuevaPosicion.x, nuevaPosicion.y, context.Transform.position.z);
    }

    private void GirarHacia(Vector3 posicionObjetivo)
    {
        // Calcula la direcci�n en 2D (X-Y)
        Vector2 direccion = (posicionObjetivo - context.Transform.position);
        float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;

        // Ajusta solo el �ngulo Z
        context.Transform.rotation = Quaternion.Euler(0, 0, angulo);
    }
}
