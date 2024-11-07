using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DynamicButtons : MonoBehaviour
{
    [SerializeField]
    private GameObject buttonPrefab;

    void Start()
    {
        GameObject buttonHost = GameObject.Instantiate(buttonPrefab, transform.GetChild(0));
        ConfigButton(buttonHost, ButtonFunc, "Click here");
    }

    private void ConfigButton(GameObject button, UnityEngine.Events.UnityAction func, string text)
    {
        button.GetComponent<Button>().onClick.AddListener(func);
        button.GetComponentInChildren<TextMeshProUGUI>().text = text;
    }

    private void ButtonFunc()
    {
        Debug.Log("El boton furula de lokos");
    }
}
