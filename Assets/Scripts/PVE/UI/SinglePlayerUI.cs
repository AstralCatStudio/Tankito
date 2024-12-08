using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading;
using Tankito;
using UnityEditorInternal;
using UnityEngine.SceneManagement;
using Tankito.SinglePlayer;

public class SinglePlayerUI : Singleton<SinglePlayerUI>
{
    [SerializeField] private GameObject ExitButton;
    [SerializeField] private GameObject CurrentWave;
    [SerializeField] private GameObject PausePanel;
    [SerializeField] private GameObject PauseButton;
    [SerializeField] private GameObject EndMenu;

    public void SetCurrentWave(int wave)
    {
        CurrentWave.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Current Wave: " + wave.ToString();
    }

    public void SetWaveTime(int time, int timeOut)
    {
        CurrentWave.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = time + " / " + timeOut + " secs ";
    }

    public void SetDefaultWaveTimer()
    {
        CurrentWave.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Spawning enemies...";
    }

    public void PauseGame()
    {
        PausePanel.SetActive(!PausePanel.activeSelf);
        if (PausePanel.activeSelf)
        {
            Time.timeScale = 0.0f;
        }
        else
        {
            Time.timeScale = 1.0f;
        }
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("SinglePlayer Menu");
        Time.timeScale = 1.0f;
    }

    public void PlayAgain()
    {
        SceneManager.LoadScene("SinglePlayer");
        Time.timeScale = 1.0f;
    }

    public void SetEndWaves()
    {
        EndMenu.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Completed waves: " + (WaveManager.Instance.CurrentWave - 1);
    }

    public void SetActiveEndMenu(bool active)
    {
        if (active)
        {
            SetEndWaves();
            EndMenu.SetActive(true);
            Time.timeScale = 0.0f;
            PauseButton.SetActive(false);
            ExitButton.SetActive(false);
            CurrentWave.SetActive(false);
        }
    }
}
