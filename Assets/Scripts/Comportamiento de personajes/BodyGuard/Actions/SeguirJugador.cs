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
            Debug.LogWarning("SeguirJugadorAction: No se encontr� un GameObject con el tag 'Player'.");
        }
    }

    // Inicializa la acci�n
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

    // L�gica principal de la acci�n
    public override Status Update()
    {
        if (jugador == null)
        {
            Debug.LogWarning("SeguirJugadorAction: Jugador no est� definido. Terminando acci�n.");
            return Status.Failure;
        }

        // Calcula la direcci�n hacia el jugador
        Vector3 direccion = (jugador.position - context.Transform.position).normalized;

        // Mueve el NPC hacia el jugador
        context.Transform.position += direccion * velocidad * Time.deltaTime;

        // Gira el NPC hacia el jugador
        GirarHacia(jugador.position);

        Debug.Log($"SeguirJugadorAction: Siguiendo al jugador en posici�n {jugador.position}.");
        return Status.Running; // La acci�n sigue ejecut�ndose
    }

    // L�gica al detener la acci�n
    public override void Stop()
    {
        Debug.Log("SeguirJugadorAction: Acci�n detenida.");
    }

    // Gira el NPC hacia una posici�n objetivo
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
