using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundUI : MonoBehaviour
{
    public GameObject PanelRanking;
    public GameObject PanelPowerUps;
    public GameObject PanelRankingFinal;

    public void SetActiveRanking(bool active)
    {
        PanelRanking.SetActive(active);
    }
    
    public void SetActivePowerUps(bool active)
    {
        PanelPowerUps.SetActive(active);
    }

    public void SetActiveRankingFinal(bool active)
    {
        PanelRankingFinal.SetActive(active);
    }
}
