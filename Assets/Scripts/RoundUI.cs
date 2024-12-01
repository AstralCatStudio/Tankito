using System;
using System.Collections;
using System.Collections.Generic;
using Tankito;
using TMPro;
using Tankito.SyncronizedButtons;
using UnityEngine;
using UnityEngine.InputSystem;

public class RoundUI : Singleton<RoundUI>
{
    public GameObject PowerUpsPrefab;

    public GameObject PanelRanking;
    public GameObject PanelPowerUps;
    public GameObject PanelRankingFinal;
    public GameObject CurrentRound;
    public GameObject CountdownText;
    public GameObject BackButton;
    public GameObject InitExitButton;
    public GameObject EndExitButton;
    public GameObject ScenarySelection;
    public GameObject LobbyInfo;
    public GameObject SettingsButton;
    public GameObject SettingsMenu;
    public GameObject Ready;
    public GameObject PlayAgain;

    #region Ranking
    public void ActivateRankingGUI(bool active)
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

    public void SetCurrentRound(int roundNumber)
    {
        CurrentRound.GetComponentInChildren<TextMeshProUGUI>().text = $"Round: {roundNumber}/{RoundManager.Instance.m_maxRounds}";
    }

    public void ActivateAliveTanksGUI(bool active)
    {
        CurrentRound.SetActive(active);
    }

    #endregion

    #region Countdown

    public void ActivateCountdownGUI(bool active)
    {
        CountdownText.SetActive(active);
    }

    public void SetCountdownText(string newText)
    {
        CountdownText.GetComponentInChildren<TextMeshProUGUI>().text = newText;
    }

    #endregion

    #region BackButton

    public void ActivateInitExitButton(bool active)
    {
        InitExitButton.SetActive(active);
    }

    public void ActivateEndExitButton(bool active)
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

    public void SetActiveScenarySelection(bool active)
    {
        ScenarySelection.SetActive(active);
    }

    public void SetScenaryText(string newText)
    {
        ScenarySelection.GetComponentInChildren<TextMeshProUGUI>().text = newText;
    }

    #endregion

    #region LobbyInfo
    internal void ActivateLobbyInfoGUI(bool active)
    {
        LobbyInfo.SetActive(active);
    }

    #endregion

    #region Settings

    public void ActivateSettingsButton(bool active)
    {
        SettingsButton.SetActive(active);
    }

    public void ActivateSettingsMenu(bool active)
    {
        SettingsMenu.SetActive(active);
    }

    public void CloseSettingsButton()
    {
        if (RoundManager.Instance.IsGameStarted)
        {
            FindObjectOfType<PlayerInput>(true).gameObject.SetActive(true);
        }
    }

    #endregion

    #region Ready

    public void ActivateReadyGUI(bool active)
    {
        Ready.GetComponent<ReadyButton>().ActivateButton(active);
    }

    #endregion

    #region PlayAgain

    public void ActivatePlayAgainGUI(bool active)
    {
        PlayAgain.GetComponent<PlayAgainButton>().ActivateButton(active);
    }

    #endregion

}
