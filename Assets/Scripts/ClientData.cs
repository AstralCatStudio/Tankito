using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ClientData : MonoBehaviour
{
    public enum accountType
    {
        guest,
        logged
    }
    private int fishScale;  //Moneda del juego (escamas)
    private int skin;   //skin que tenga el jugador puesta. Actualmente es un int pero habrá que hacer que sea una clase o algun tipo para referenciar a la skin
    private accountType accountT;   //Tipo de la cuenta al iniciar sesión
    public UnityEvent menuEvents;   //Variable que se suscribirá a distintos eventos.

    // Start is called before the first frame update
    void Start()
    {
        fishScale = 0;
        accountT = 0;
        skin = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
