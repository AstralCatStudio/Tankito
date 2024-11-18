using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourAPI.Core;
using BehaviourAPI.UnityToolkit;

[SelectionGroup("BodyGuardActions")]
public class Apuntar : UnityAction
{
    private Transform jugador; // Referencia al jugador
    private float velocidadRotacion = 5f; // Velocidad de rotaci�n en grados por segundo

    protected override void OnSetContext()
    {
        GameObject jugadorObj = GameObject.FindWithTag("Player");
        if (jugadorObj != null)
        {
            jugador = jugadorObj.transform;
        }
        else
        {
            Debug.LogWarning("Apuntar: No se encontr� un GameObject con el tag 'Player'.");
        }
    }

    public override void Start()
    {
        Debug.Log("Apuntar: Iniciando acci�n para apuntar al jugador.");
    }

    public override Status Update()
    {
        if (jugador == null)
        {
            Debug.LogWarning("Apuntar: Jugador no est� definido. Terminando acci�n.");
            return Status.Failure;
        }

        // Calcula el �ngulo hacia el jugador
        Vector2 direccion = (jugador.position - context.Transform.position).normalized;
        float anguloObjetivo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;

        // Rotaci�n suave hacia el jugador
        float anguloActual = context.Transform.rotation.eulerAngles.z;
        float nuevoAngulo = Mathf.MoveTowardsAngle(anguloActual, anguloObjetivo, velocidadRotacion * Time.deltaTime);
        context.Transform.rotation = Quaternion.Euler(0, 0, nuevoAngulo);

        // Verifica si el NPC ya est� alineado con el jugador
        bool estaApuntando = Mathf.Abs(Mathf.DeltaAngle(anguloActual, anguloObjetivo)) < 2f;
        Debug.Log($"Apuntar: �ngulo actual = {anguloActual}, Objetivo = {anguloObjetivo}, Alineado = {estaApuntando}");
        return estaApuntando ? Status.Success : Status.Running;
    }

    public override void Stop()
    {
        Debug.Log("Apuntar: Acci�n detenida.");
    }
}
