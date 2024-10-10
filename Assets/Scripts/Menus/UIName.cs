using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UIName : MonoBehaviour
{
    public TMP_InputField inputName;
    public Button acceptButton;

    private void Start()
    {
        if (inputName == null)
        {
            Debug.Log("InputField error");
        }

        if(acceptButton == null)
        {
            Debug.Log("Button error");
        }
    }

    public void accept()
    {
        string name = inputName.text;
        if(name != "")
        {
            GameManager.Instance.SetPlayerName(name);
            SceneManager.LoadScene("StartMenu");
        }
        else
        {
            Debug.Log("Nombre vacio");
        }
        
    }

}
