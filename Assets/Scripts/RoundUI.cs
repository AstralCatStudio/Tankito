using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoundUI : MonoBehaviour
{
    public GameObject PanelRanking;
    public GameObject PanelPowerUps;
    public GameObject PanelRankingFinal;
    public GameObject PanelAlivePlayers;

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

    public void SetRankingText(string text)
    {
        PanelRanking.GetComponentInChildren<TextMeshProUGUI>().text = text;
    }

    public void SetRemainingPlayers(int players)
    {
        PanelAlivePlayers.GetComponentInChildren<TextMeshProUGUI>().text = $"Remaining players: {players}";
    }

    public void SetRemainingPlayersActive(bool active)
    {
        PanelAlivePlayers.SetActive(active);
    }
}
