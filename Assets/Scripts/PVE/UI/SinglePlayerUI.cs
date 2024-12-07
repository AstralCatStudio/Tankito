using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading;

public class SinglePlayerUI : Singleton<SinglePlayerUI>
{
    [SerializeField] private GameObject CurrentWave;

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
}
