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
            int pingMS = (int)(SimulationParameters.CURRENT_LATENCY * 1000);
            if (pingMS > 0)
            {
                text.text = pingMS + "ms";
            }
            else
            {
                text.text = "";
            }
            m_timeAccumulator = 0;
        }
    }

}
