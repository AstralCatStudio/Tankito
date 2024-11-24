using System.Collections;
using System.Collections.Generic;
using Tankito;
using TMPro;
using UnityEngine;

public class RoundUI : Singleton<RoundUI>
{
    public GameObject PanelRanking;
    public GameObject PanelPowerUps;
    public GameObject PanelRankingFinal;
    public GameObject PanelAlivePlayers;
    public GameObject CountdownText;
    public GameObject BackButton;

    #region Ranking
    public void SetActiveRanking(bool active)
    {
        PanelRanking.SetActive(active);
    }

    public void SetRankingText(string text)
    {
        PanelRanking.GetComponentInChildren<TextMeshProUGUI>().text = text;
    }

    #endregion

    #region Powerups

    public void SetActivePowerUps(bool active)
    {
        PanelPowerUps.SetActive(active);
    }

    #endregion

    #region RankingFinal

    public void SetActiveRankingFinal(bool active)
    {
        PanelRankingFinal.SetActive(active);
    }

    #endregion

    #region RemainingPlayers

    public void SetRemainingPlayers(int players)
    {
        PanelAlivePlayers.GetComponentInChildren<TextMeshProUGUI>().text = $"Remaining players: {players}";
    }

    public void ActivateAliveTanksGUI(bool active)
    {
        PanelAlivePlayers.SetActive(active);
    }

    #endregion

    #region Countdown

    public void SetActiveCountownText(bool active)
    {
        CountdownText.SetActive(active);
    }

    public void SetCountdownText(string newText)
    {
        CountdownText.GetComponentInChildren<TextMeshProUGUI>().text = newText;
    }

    #endregion

    #region BackButton

    public void SetActiveBackButton(bool active)
    {
        BackButton.SetActive(active);
    }

    public void Back()
    {
        Debug.Log("Pulsaste Back");

        DisconnectHandler.Instance.ExitGame();
    }

    #endregion
}
