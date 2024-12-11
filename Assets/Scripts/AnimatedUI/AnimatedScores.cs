using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Tankito;

//Clase para probar 
public class PlayerTest : IComparable
{
    public string name;
    public Color color;
    public Sprite icon;
    public int score;
    public int position;
    public List<Sprite> modifiers = new();
    public GameObject playerInfo = new();

    //Sort method
    public int CompareTo(object obj)
    {
        var a = this;
        var b = obj as PlayerTest;

        if (a.score < b.score)
            return 1;
        else if (a.score > b.score)
            return -1;

        return 0;
    }
}

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

    //#region removeThis
    //[Header("Remove this params")]
    //private List<PlayerTest> players = new();  //Esto se podrá eliminar
    //private Color[] colors = { Color.blue, Color.red, Color.green, Color.yellow };
    //[SerializeField] private Sprite[] icons;
    //#endregion


    #region UnityFunctions
    private void Awake()
    {
        //-------------------------
        //Esto habrá que eliminarlo
        //for (int i= 0; i < numPlayers;i++)
        //{
        //    PlayerTest player = new PlayerTest();
        //    player.name = "Player " + (i+1).ToString();
        //    player.color = colors[i];
        //    player.icon = icons[i];
        //    player.score = 0;
        //    players.Add(player);
        //}
        //Esto habrá que eliminarlo
        //-------------------------

        //foreach (PlayerTest player in players)   //Cambiarlo para que se haga por cada jugador que haya en partida (Creo que por los TankDatas)
        //{
        //    GameObject instance = Instantiate(sliderPrefab, panelRT);
        //    LoadInfo(instance, player);
        //    sliderScores.Add(instance);
        //}
        //LeanTween.scale(panelRT, Vector2.zero, 0f);
    }

    private void OnEnable()
    {
        LeanTween.scale(panelRT, Vector2.one, popupTime).setEase(LeanTweenType.easeOutElastic);
        //for (int i = 0; i < sliderScores.Count; i++)  //numero de jugadores
        //{
        //    players[i].score += UnityEngine.Random.Range(0, 4);
        //}
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

    /// <summary>
    /// Carga la información del slider del jugador
    /// </summary>
    /// <param name="instance"></param>
    /// <param name="player"></param>
    private void LoadInfo(GameObject instance, PlayerTest player)   //Cambiar al TankData
    {
        SliderLoadInfo infoToLoad = instance.GetComponent<SliderLoadInfo>();
        infoToLoad.icon.sprite = player.icon;
        infoToLoad.name.text = player.name;
        infoToLoad.name.color = player.color;
        infoToLoad.fill.color = player.color;
        infoToLoad.slider.value = player.score;
        infoToLoad.score.text = player.score.ToString();
    }

    public void AnimateScores()
    {
        for (int i = 0; i < tankList.Count; i++)  //numero de jugadores
        {
            AnimateSlider(tankList[i], sliderScores[i]);
        }
    }

    private void AnimateSlider(PlayerTest player, GameObject slider) //Cambiar a TankData
    {
        SliderLoadInfo sliderInfo = slider.GetComponent<SliderLoadInfo>();
        StartCoroutine(LerpSlider(sliderInfo, sliderInfo.slider.value, player.score, sliderTime));
    }

    private void AnimateSlider(TankData tank, GameObject slider) //Cambiar a TankData
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
        //Invoke("Disable", popupTime);
        ResetScreen();
    }

    private void Disable()
    {
        gameObject.SetActive(false);
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
