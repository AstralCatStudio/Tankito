using System;
using System.Collections;
using System.Collections.Generic;
using Tankito;
using TMPro;
using Tankito.SyncronizedButtons;
using UnityEngine;
using UnityEngine.InputSystem;
using Tankito.ScenarySelection;

public class RoundUI : Singleton<RoundUI>
{
    public GameObject PowerUpsPrefab;

    public GameObject SlidersRanking;
    public GameObject PanelPowerUps;
    public GameObject CurrentRound;
    public GameObject CountdownText;
    public GameObject InitExitButton;
    public GameObject EndExitButton;
    public GameObject ScenarySelection;
    public GameObject LobbyInfo;
    public GameObject SettingsButton;
    public GameObject SettingsMenu;
    public GameObject Ready;
    public GameObject PlayAgain;
    public GameObject WinScreen;

    #region Ranking

    public void InitRanking(List<TankData> tanks)
    {
        SlidersRanking.GetComponent<AnimatedScores>().InitScoreSliders(tanks);
    }

    public void ActivateRankingGUI(bool active)
    {
        SlidersRanking.SetActive(active);
    }

    public void SetRankingText(string text)
    {
        SlidersRanking.GetComponentInChildren<TextMeshProUGUI>().text = text;
    }

    #endregion

    #region Powerups

    public void SetActivePowerUps(bool active)
    {
        
        PanelPowerUps.SetActive(active);
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

        ///////////////////////////////////////////////////////////////////// sound effect
        MusicManager.Instance.PlaySound("aceptar2");

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

    public void NextScenary()
    {
        ScenarySelector.Instance.NextScenary();
    }
    
    public void PreviousScenary()
    {
        ScenarySelector.Instance.PreviousScenary();
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
        ///////////////////////////////////////////////////////////////////// sound effect
        MusicManager.Instance.PlaySound("aceptar");

        SettingsMenu.SetActive(active);
    }

    public void CloseSettingsButton()
    {
        if (RoundManager.Instance.IsGameStarted)
        {
            ///////////////////////////////////////////////////////////////////// sound effect
            MusicManager.Instance.PlaySound("aceptar2");

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

    #region WinScreen

    public void InitWinScreen(List<TankData> tanks)
    {
        WinScreen.GetComponent<WinMenu>().InitWinMenu(tanks);
    }

    public void ResetWinScreen()
    {
        WinScreen.GetComponent<WinMenu>().ResetMenu();
    }

    public void ActivateWinScreen(bool active)
    {
        WinScreen.SetActive(active);
    }

    #endregion

}
