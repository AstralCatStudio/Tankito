using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputFieldUsername : MonoBehaviour
{
    public void ChangeUsername()
    {
        string username = GetComponent<TMP_InputField>().text.ToString();
        ClientData.Instance.ChangeUsername(username);
    }

    public void KeyPress()
    {
        MusicManager.Instance.PlaySound("bip");
    }
}
