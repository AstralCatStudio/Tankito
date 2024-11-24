using System.Collections;
using System.Collections.Generic;
using Tankito.Netcode;
using TMPro;
using UnityEngine;

public class ChangeWorstLatency : MonoBehaviour
{
    TextMeshProUGUI text;

    private void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        text.text = ((int)(Parameters.WORST_CASE_LATENCY * 1000)).ToString() + "ms";
    }

    public void OnWorstLatencyChange(float value)
    {
        GetComponent<TextMeshProUGUI>().text = value.ToString() + "ms";
        Parameters.WORST_CASE_LATENCY = (double)value/1000;
    }
}
