using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading;
using Tankito;
using UnityEditorInternal;
using UnityEngine.SceneManagement;

public class SinglePlayerUI : Singleton<SinglePlayerUI>
{
    [SerializeField] private GameObject CurrentWave;
    [SerializeField] private GameObject PausePanel;

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
        if(PausePanel.activeSelf)
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
        SceneManager.LoadScene("MainMenu");
    }
}
