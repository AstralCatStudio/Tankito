using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UsernameText : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ClientData.Instance.onUsernameChanged += UpdateUsername;
        GetComponent<TextMeshProUGUI>().text = ClientData.Instance.username;
    }
    
    private void UpdateUsername()
    {
        GetComponent<TextMeshProUGUI>().text = ClientData.Instance.username;
    }
}
