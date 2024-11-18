using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourAPI.Core;
using BehaviourAPI.UnityToolkit;

[SelectionGroup("BodyGuardActions")]
public class Disparar : UnityAction
{
    public GameObject proyectilPrefab; // Prefab del proyectil
    public Transform puntoDisparo; // Punto desde donde se dispara el proyectil
    private int balasDisponibles = 5; // Balas disponibles

    protected override void OnSetContext()
    {
        // Configuraci�n opcional si se requiere el contexto
        Debug.Log("Disparar: Acci�n configurada.");
    }

    public override void Start()
    {
        Debug.Log("Disparar: Iniciando acci�n para disparar.");
    }

    public override Status Update()
    {
        /*
        if (balasDisponibles <= 0)
        {
            Debug.LogWarning("Disparar: No quedan balas.");
            return Status.Failure;
        }*/

        if (proyectilPrefab == null || puntoDisparo == null)
        {
            Debug.LogError("Disparar: No se ha configurado el prefab del proyectil o el punto de disparo.");
            return Status.Failure;
        }

        // Instancia el proyectil
        GameObject proyectil = Object.Instantiate(proyectilPrefab, puntoDisparo.position, puntoDisparo.rotation);
        Debug.Log("Disparar: Proyectil disparado.");

        // Reduce el n�mero de balas
        balasDisponibles--;

        return Status.Success; // Acci�n completada
    }

    public override void Stop()
    {
        Debug.Log("Disparar: Acci�n detenida.");
    }
}
