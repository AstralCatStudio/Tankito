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
        // Configuración opcional si se requiere el contexto
        Debug.Log("Disparar: Acción configurada.");
    }

    public override void Start()
    {
        Debug.Log("Disparar: Iniciando acción para disparar.");
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

        // Reduce el número de balas
        balasDisponibles--;

        return Status.Success; // Acción completada
    }

    public override void Stop()
    {
        Debug.Log("Disparar: Acción detenida.");
    }
}
