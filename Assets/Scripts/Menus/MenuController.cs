using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MenuController : Singleton<MenuController>
{
    [Serializable]
    public enum menuName
    {
        InitialScreenLogIn = 0,
        MainMenu = 1,
        Settings = 2,
        Credits = 3,
        LogOut = 4,
        Shop = 5,
        PlayMenu = 6,
        Lobby = 7,
        CharacterSelection = 8
    }
    public GameObject[] menus;
    public GameObject[] bgMenus;
    public Vector2[] bgPositions; //Posiciones del "grid"
    public Animator animator;
    public ParticleSystem bubbleParticles;
    
    
    public int currentMenuIndex;
    public float xUnit = 56, yUnit = 30;  
    public float bgParallaxFactor = 1.2f, mgParallaxFactor = 1f, fgParallaxFactor = 0.7f;

    public UnityEvent<Vector2,GameObject,GameObject> menuChanged;

    // Start is called before the first frame update
    void Start()
    {
        BgReposition();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeToMenu(int newMenuIndex)
    {
        animator.SetInteger("Menu", newMenuIndex);
        MoveBG(newMenuIndex);
        currentMenuIndex = newMenuIndex;
    }

    private void MoveBG(int newMenuIndex)
    {
        Transform bg;
        Transform mg;
        Transform fg;
        //newTranslation determina la direccion en la que se mueven los fondos
        Vector2 newTranslation = bgPositions[currentMenuIndex] - bgPositions[newMenuIndex];
        Vector2 bubbleDirection = newTranslation.normalized;

        // Calcula el ángulo en el plano 2D
        float angle = Mathf.Atan2(-bubbleDirection.x, bubbleDirection.y) * Mathf.Rad2Deg;
        // Aplica la rotación al sistema de partículas
        bubbleParticles.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        bubbleDirection = Quaternion.LookRotation(bubbleDirection).eulerAngles;
        //Debug.Log(bubbleDirection.ToString());
        //bubbleParticles.transform.Rotate(new Vector3(0f, 0f, bubbleDirection.x));

        bubbleParticles.Play();

        menuChanged?.Invoke(newTranslation, bgMenus[currentMenuIndex], bgMenus[newMenuIndex]);

        for (int i = 0; i < bgMenus.Length; i++)
        {
            if(i == currentMenuIndex || i == newMenuIndex)
            {
                continue;
            }
            bg = bgMenus[i].transform.GetChild(0);
            mg = bgMenus[i].transform.GetChild(1);
            fg = bgMenus[i].transform.GetChild(2);

            bg.Translate(xUnit * bgParallaxFactor * newTranslation.x, yUnit * bgParallaxFactor * newTranslation.y, 0);
            mg.Translate(xUnit * mgParallaxFactor * newTranslation.x, yUnit * mgParallaxFactor * newTranslation.y, 0);
            fg.Translate(xUnit * fgParallaxFactor * newTranslation.x, yUnit * fgParallaxFactor * newTranslation.y, 0);
        }
    }

    public void CheckPasswordAndUsername()
    {
        //Hacer comprobaciones del nombre y la contraseña
    }

    public void EnterAsGuest()
    {
        //Esto activa que el jugador está como invitado
    }

    private void BgReposition()
    {
        Transform bg;
        Transform mg;
        Transform fg;
        for(int i=0; i < bgMenus.Length; i++)
        {
            bg = bgMenus[i].transform.GetChild(0);
            mg = bgMenus[i].transform.GetChild(1);
            fg = bgMenus[i].transform.GetChild(2);

            bg.Translate(xUnit * bgParallaxFactor * bgPositions[i].x, yUnit * bgParallaxFactor * bgPositions[i].y, 0);
            mg.Translate(xUnit * mgParallaxFactor * bgPositions[i].x, yUnit * mgParallaxFactor * bgPositions[i].y, 0);
            fg.Translate(xUnit * fgParallaxFactor * bgPositions[i].x, yUnit * fgParallaxFactor * bgPositions[i].y, 0);
        }
    }
}