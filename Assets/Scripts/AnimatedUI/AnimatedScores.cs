using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Tankito;

public class AnimatedScores : MonoBehaviour
{
    [Header("Params")]
    #region parameters
    [SerializeField] private float popupTime = 0.5f;
    [SerializeField] private float waitTime = 0.5f;
    [SerializeField] private float sliderTime = 0.5f;
    [SerializeField, Range(2, 4)] private int numPlayers = 2;
    [SerializeField] private RectTransform panelRT;
    [SerializeField] private GameObject sliderPrefab;
    [SerializeField] private List<GameObject> sliderScores;
    [SerializeField] private List<TankData> tankList;
    #endregion

    #region UnityFunctions
    
    private void OnEnable()
    {
        LeanTween.scale(panelRT, Vector2.one, popupTime).setEase(LeanTweenType.easeOutElastic);
        Invoke("AnimateScores", waitTime);
    }
    #endregion

    public void InitScoreSliders(List<TankData> tanks)
    {
        foreach (TankData tank in tanks)
        {
            GameObject newSlider = Instantiate(sliderPrefab, panelRT);
            LoadInfo(newSlider, tank);
            sliderScores.Add(newSlider);
            tankList.Add(tank);
        }
        LeanTween.scale(panelRT, Vector2.zero, 0f);
    }

    private void LoadInfo(GameObject instance, TankData tank)
    {
        SliderLoadInfo infoToLoad = instance.GetComponent<SliderLoadInfo>();
        infoToLoad.icon.sprite = ClientData.Instance.characters[tank.SkinSelected].data.sprite;
        infoToLoad.name.text = tank.Username;
        infoToLoad.name.color = tank.playerColor;
        infoToLoad.fill.color = tank.playerColor;
        infoToLoad.slider.value = tank.Points;
        infoToLoad.score.text = tank.Points.ToString();
    }

    public void AnimateScores()
    {
        for (int i = 0; i < tankList.Count; i++)  //numero de jugadores
        {
            AnimateSlider(tankList[i], sliderScores[i]);
        }
    }

    private void AnimateSlider(TankData tank, GameObject slider)
    {
        SliderLoadInfo sliderInfo = slider.GetComponent<SliderLoadInfo>();
        StartCoroutine(LerpSlider(sliderInfo, sliderInfo.slider.value, tank.Points, sliderTime));
    }

    private IEnumerator LerpSlider(SliderLoadInfo sliderInfo, float from, float to, float time)
    {
        float i = 0;
        while (i < time)
        {
            sliderInfo.slider.value = Mathf.Lerp(from, to, i / time);
            sliderInfo.score.text = Mathf.Round(sliderInfo.slider.value).ToString();
            i += Time.deltaTime;
            yield return null;
        }
        Mathf.Round(sliderInfo.slider.value);

        Invoke("Disappear", waitTime);
    }

    private void Disappear()
    {
        LeanTween.scale(panelRT, Vector2.zero, popupTime).setEase(LeanTweenType.easeInBack);
        ResetScreen();
    }

    private void ResetScreen()
    {
        foreach(GameObject slider in sliderScores)
        {
            Destroy(slider);
        }
        sliderScores.Clear();

        tankList.Clear();
    }
}
