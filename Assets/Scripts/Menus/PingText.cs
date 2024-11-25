using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Tankito.Netcode;
using Tankito;

public class PingText : MonoBehaviour
{
    TextMeshProUGUI text;
    int m_currentTick;
    int m_ticksToDebug;

    private void OnEnable()
    {
        SimClock.OnTick += Debug;
    }

    private void OnDisable()
    {
        SimClock.OnTick -= Debug;
    }

    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        m_ticksToDebug = SimulationParameters.SIM_TICK_RATE;
    }

    void Debug()
    {
        m_currentTick++;
        if(m_currentTick >= m_ticksToDebug)
        {
            text.text = (int)(SimulationParameters.CURRENT_LATENCY * 1000) + "ms";
            m_currentTick = 0;
        }
    }

}
