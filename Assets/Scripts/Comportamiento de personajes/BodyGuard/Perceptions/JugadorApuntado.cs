using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourAPI.Core;
using BehaviourAPI.UnityToolkit;

[SelectionGroup("BodyGuardPerceptions")]
public class JugadorApuntado : UnityPerception
{
    private Transform jugador; // Referencia al jugador
    private float toleranciaAngulo = 5f; // Margen de error en grados para considerar que el jugador est� apuntado

    protected override void OnSetContext()
    {
        GameObject jugadorObj = GameObject.FindWithTag("Player");
        if (jugadorObj != null)
        {
            jugador = jugadorObj.transform;
            Debug.Log("JugadorApuntado: Jugador encontrado correctamente.");
        }
        else
        {
            Debug.LogWarning("JugadorApuntado: No se encontr� un GameObject con el tag 'Player'.");
        }
    }

    public override void Initialize()
    {
        Debug.Log("JugadorApuntado: Percepci�n inicializada.");
    }

    public override bool Check()
    {
        if (jugador == null)
        {
            Debug.LogWarning("JugadorApuntado: Jugador no est� definido.");
            return false;
        }

        // Calcula la direcci�n hacia el jugador
        Vector2 direccionHaciaJugador = (jugador.position - context.Transform.position).normalized;

        // Obtiene la direcci�n actual del NPC
        Vector2 direccionNPC = context.Transform.right; // En 2D, "right" es la direcci�n hacia donde apunta el objeto

        // Calcula el �ngulo entre las dos direcciones
        float angulo = Vector2.Angle(direccionHaciaJugador, direccionNPC);

        // Comprueba si el �ngulo est� dentro del margen de tolerancia
        bool apuntado = angulo <= toleranciaAngulo;

        Debug.Log($"JugadorApuntado: �ngulo actual = {angulo}, Tolerancia = {toleranciaAngulo}, Apuntado = {apuntado}");
        return apuntado;
    }

    public override void Reset()
    {
        Debug.Log("JugadorApuntado: Reiniciando percepci�n.");
    }
}
