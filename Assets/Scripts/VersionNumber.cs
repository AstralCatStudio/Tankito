using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VersionNumber : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI versionGUI;
    void Start()
    {
        versionGUI.text = Application.version;
    }
}
