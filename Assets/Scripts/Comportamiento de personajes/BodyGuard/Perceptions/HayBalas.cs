using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourAPI.Core;
using BehaviourAPI.UnityToolkit;

[SelectionGroup("BodyGuardPerceptions")]
public class HayBalas : UnityPerception
{
    private int balasDisponibles = 5; // N�mero de balas inicial

    public override void Initialize()
    {
        Debug.Log("HayBalas: Percepci�n inicializada.");
    }

    public override bool Check()
    {
        bool tieneBalas = balasDisponibles > 0;
        Debug.Log($"HayBalas: Balas disponibles = {balasDisponibles}, Tiene balas = {tieneBalas}");
        return tieneBalas;
    }

    public override void Reset()
    {
        Debug.Log("HayBalas: Reiniciando percepci�n.");
    }

    // M�todo auxiliar para reducir balas despu�s de disparar
    public void ReducirBalas()
    {
        if (balasDisponibles > 0)
        {
            balasDisponibles--;
        }
    }

    // M�todo auxiliar para recargar balas
    public void Recargar(int cantidad)
    {
        balasDisponibles += cantidad;
    }
}
