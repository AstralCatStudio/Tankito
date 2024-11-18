using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourAPI.Core;
using BehaviourAPI.UnityToolkit;

[SelectionGroup("BodyGuardPerceptions")]
public class JugadorApuntado : UnityPerception
{
    private Transform jugador; // Referencia al jugador
    private float toleranciaAngulo = 5f; // Margen de error en grados para considerar que el jugador está apuntado

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
            Debug.LogWarning("JugadorApuntado: No se encontró un GameObject con el tag 'Player'.");
        }
    }

    public override void Initialize()
    {
        Debug.Log("JugadorApuntado: Percepción inicializada.");
    }

    public override bool Check()
    {
        if (jugador == null)
        {
            Debug.LogWarning("JugadorApuntado: Jugador no está definido.");
            return false;
        }

        // Calcula la dirección hacia el jugador
        Vector2 direccionHaciaJugador = (jugador.position - context.Transform.position).normalized;

        // Obtiene la dirección actual del NPC
        Vector2 direccionNPC = context.Transform.right; // En 2D, "right" es la dirección hacia donde apunta el objeto

        // Calcula el ángulo entre las dos direcciones
        float angulo = Vector2.Angle(direccionHaciaJugador, direccionNPC);

        // Comprueba si el ángulo está dentro del margen de tolerancia
        bool apuntado = angulo <= toleranciaAngulo;

        Debug.Log($"JugadorApuntado: Ángulo actual = {angulo}, Tolerancia = {toleranciaAngulo}, Apuntado = {apuntado}");
        return apuntado;
    }

    public override void Reset()
    {
        Debug.Log("JugadorApuntado: Reiniciando percepción.");
    }
}
