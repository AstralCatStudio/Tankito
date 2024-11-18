using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourAPI.Core;
using BehaviourAPI.UnityToolkit;

[SelectionGroup("BodyGuardActions")]
public class Apuntar : UnityAction
{
    private Transform jugador; // Referencia al jugador
    private float velocidadRotacion = 5f; // Velocidad de rotación en grados por segundo

    protected override void OnSetContext()
    {
        GameObject jugadorObj = GameObject.FindWithTag("Player");
        if (jugadorObj != null)
        {
            jugador = jugadorObj.transform;
        }
        else
        {
            Debug.LogWarning("Apuntar: No se encontró un GameObject con el tag 'Player'.");
        }
    }

    public override void Start()
    {
        Debug.Log("Apuntar: Iniciando acción para apuntar al jugador.");
    }

    public override Status Update()
    {
        if (jugador == null)
        {
            Debug.LogWarning("Apuntar: Jugador no está definido. Terminando acción.");
            return Status.Failure;
        }

        // Calcula el ángulo hacia el jugador
        Vector2 direccion = (jugador.position - context.Transform.position).normalized;
        float anguloObjetivo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;

        // Rotación suave hacia el jugador
        float anguloActual = context.Transform.rotation.eulerAngles.z;
        float nuevoAngulo = Mathf.MoveTowardsAngle(anguloActual, anguloObjetivo, velocidadRotacion * Time.deltaTime);
        context.Transform.rotation = Quaternion.Euler(0, 0, nuevoAngulo);

        // Verifica si el NPC ya está alineado con el jugador
        bool estaApuntando = Mathf.Abs(Mathf.DeltaAngle(anguloActual, anguloObjetivo)) < 2f;
        Debug.Log($"Apuntar: Ángulo actual = {anguloActual}, Objetivo = {anguloObjetivo}, Alineado = {estaApuntando}");
        return estaApuntando ? Status.Success : Status.Running;
    }

    public override void Stop()
    {
        Debug.Log("Apuntar: Acción detenida.");
    }
}
