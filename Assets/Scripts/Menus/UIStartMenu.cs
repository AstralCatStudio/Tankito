using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIStartMenu : MonoBehaviour
{

    public void Play()
    {
        Debug.Log("Play");
        SceneManager.LoadScene("LoginMenu");
    }

    public void Options()
    {
        Debug.Log("Menu de opciones");
    }

    public void Quit()
    {
        Debug.Log("Salir del juego");
    }

}
