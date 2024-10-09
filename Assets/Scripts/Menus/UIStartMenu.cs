using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class UIStartMenu : MonoBehaviour
{
    //public TextMeshProUGUI _playerName;

    private void Start()
    {
        /*_playerName = gameObject.transform.Find("Panel/PlayerName").GetComponent<TextMeshProUGUI>();
        _playerName.text = GameManager.Instance.GetPlayerName();*/

        UIFadeComponent _fadeTitle= gameObject.transform.Find("TitlePanel").GetComponent<UIFadeComponent>();
        if (_fadeTitle == null)
        {
            Debug.Log("No encontró el fade");
        }
        _fadeTitle.FadeIn();
    }


    /*
     * 
     * Buttons
     * 
     */

    public void Play()
    {
        Debug.Log("Play");
        SceneManager.LoadScene("PlayMenu");
    }

    public void Options()
    {
        Debug.Log("Menu de opciones");
    }

    public void Quit()
    {
        Debug.Log("Salir del juego");
    }

    public void Login()
    {
        Debug.Log("Login");
        SceneManager.LoadScene("LoginMenu");
    }

}
