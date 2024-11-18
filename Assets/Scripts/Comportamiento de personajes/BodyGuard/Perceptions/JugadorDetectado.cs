using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourAPI.Core;
using BehaviourAPI.UnityToolkit;

[SelectionGroup("BodyGuardPerceptions")]
public class JugadorDetectado : UnityPerception
{
    private Transform jugador; // Referencia al transform del jugador
    private float rangoDeteccion = 10f; // Rango de detección del jugador

    protected override void OnSetContext()
    {
        GameObject jugadorObj = GameObject.FindWithTag("Player");
        if (jugadorObj != null)
        {
            jugador = jugadorObj.transform;
            Debug.Log("JugadorDetectado: Jugador encontrado correctamente.");
        }
        else
        {
            Debug.LogWarning("JugadorDetectado: No se encontró un GameObject con el tag 'Player'.");
        }

        rangoDeteccion = 10f; // Ajustar rango según diseño
    }

    public override void Initialize()
    {
        Debug.Log("JugadorDetectado: Percepción inicializada.");
    }

    public override bool Check()
    {
        if (jugador == null)
        {
            Debug.LogWarning("JugadorDetectado: Jugador es null. ¿Está configurado el tag correctamente?");
            return false;
        }

        // Si el juego es top-down, ignora la altura (eje Y)
        float distancia = Vector3.Distance(
            new Vector3(context.Transform.position.x, 0, context.Transform.position.z),
            new Vector3(jugador.position.x, 0, jugador.position.z)
        );

        bool detectado = distancia <= rangoDeteccion;

        Debug.Log($"JugadorDetectado: Distancia al jugador = {distancia}, Detectado = {detectado}");
        return detectado;
    }

    public override void Reset()
    {
        Debug.Log("JugadorDetectado: Reiniciando percepción.");
    }
}
