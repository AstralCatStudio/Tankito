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

    // Configura el contexto para obtener referencias necesarias
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
            Debug.LogWarning("SeguirJugadorAction: No se encontró un GameObject con el tag 'Player'.");
        }
    }

    // Inicializa la acción
    public override void Start()
    {
        if (jugador == null)
        {
            Debug.LogError("SeguirJugadorAction: No se puede ejecutar la acción porque el jugador es null.");
        }
        else
        {
            Debug.Log("SeguirJugadorAction: Iniciando acción de seguimiento al jugador.");
        }
    }

    // Lógica principal de la acción
    public override Status Update()
    {
        if (jugador == null)
        {
            Debug.LogWarning("SeguirJugadorAction: Jugador no está definido. Terminando acción.");
            return Status.Failure;
        }

        // Calcula la dirección hacia el jugador
        Vector3 direccion = (jugador.position - context.Transform.position).normalized;

        // Mueve el NPC hacia el jugador
        context.Transform.position += direccion * velocidad * Time.deltaTime;

        // Gira el NPC hacia el jugador
        GirarHacia(jugador.position);

        Debug.Log($"SeguirJugadorAction: Siguiendo al jugador en posición {jugador.position}.");
        return Status.Running; // La acción sigue ejecutándose
    }

    // Lógica al detener la acción
    public override void Stop()
    {
        Debug.Log("SeguirJugadorAction: Acción detenida.");
    }

    // Gira el NPC hacia una posición objetivo
    private void GirarHacia(Vector3 posicionObjetivo)
    {
        Vector3 direccion = (posicionObjetivo - context.Transform.position).normalized;
        Quaternion rotacionObjetivo = Quaternion.LookRotation(direccion);
        context.Transform.rotation = Quaternion.Slerp(
            context.Transform.rotation,
            rotacionObjetivo,
            5f * Time.deltaTime // Velocidad de giro
        );
    }
}
