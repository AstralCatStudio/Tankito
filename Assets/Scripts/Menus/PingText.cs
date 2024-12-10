using TMPro;
using UnityEngine;
using Tankito.Netcode;

public class PingText : MonoBehaviour
{
    TextMeshProUGUI text;
    float m_timeAccumulator;
    [SerializeField] float m_logRate = 3;


    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        m_logRate = SimulationParameters.SIM_TICK_RATE;
    }

    void Update()
    {

        m_timeAccumulator += Time.deltaTime;
        if(m_timeAccumulator >= m_logRate)
        {
            text.text = (int)(SimulationParameters.CURRENT_LATENCY * 1000) + "ms";
            m_timeAccumulator = 0;
        }
    }

}
