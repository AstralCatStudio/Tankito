using System.Collections;
using System.Collections.Generic;
using Tankito;
using TMPro;
using UnityEngine;

public class GetJoinCode : MonoBehaviour
{
    void Start()
    {
        GetComponent<TextMeshProUGUI>().text = "JOINCODE: " + GameManager.Instance.joinCode;
    }
}