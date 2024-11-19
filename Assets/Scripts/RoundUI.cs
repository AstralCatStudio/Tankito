using System.Collections;
using System.Collections.Generic;
using Tankito;
using TMPro;
using UnityEngine;

public class RoundUI : MonoBehaviour
{
    public GameObject PanelRanking;
    public GameObject PanelPowerUps;
    public GameObject PanelRankingFinal;
    public GameObject PanelAlivePlayers;
    public GameObject CountdownText;
    public GameObject InitExitButton;
    public GameObject EndExitButton;
    public GameObject ScenarySelection;

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

    public void SetRemainingPlayersActive(bool active)
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

    public void SetActiveInitExitButton(bool active)
    {
        InitExitButton.SetActive(active);
    }
    
    public void SetActiveEndExitButton(bool active)
    {
        EndExitButton.SetActive(active);
    }

    public void Exit()
    {
        Debug.Log("Pulsaste Exit");

        DisconnectHandler.Instance.ExitGame();
    }

    #endregion

    #region Scenary

    public void SetActiveScenaryButtons(bool active)
    {
        ScenarySelection.SetActive(active);
    }

    #endregion
}
