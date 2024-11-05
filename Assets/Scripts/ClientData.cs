using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ClientData : Singleton<ClientData>
{
    public enum accountType
    {
        guest,
        logged
    }
    public int money;  //Moneda del juego (escamas)
    public int skin;   //skin que tenga el jugador puesta. Actualmente es un int pero habrá que hacer que sea una clase o algun tipo para referenciar a la skin
    public accountType accountT;   //Tipo de la cuenta al iniciar sesión
    public UnityEvent onMoneyChanged;

    // Start is called before the first frame update
    void Start()
    {
        money = 0;
        accountT = 0;
        skin = 0;
    }

    public void ChangeMoney(int moneyAdded)
    {
        money += moneyAdded;
        money = Mathf.Clamp(money, 0, 999999999);
        onMoneyChanged?.Invoke();
    }
}
