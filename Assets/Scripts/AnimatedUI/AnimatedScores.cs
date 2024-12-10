using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    #endregion

    #region removeThis
    [Header("Remove this params")]
    private List<PlayerTest> players = new();  //Esto se podrá eliminar
    private Color[] colors = { Color.blue, Color.red, Color.green, Color.yellow };
    [SerializeField] private Sprite[] icons;
    #endregion


    #region UnityFunctions
    private void Awake()
    {
        //-------------------------
        //Esto habrá que eliminarlo
        for (int i= 0; i < numPlayers;i++)
        {
            PlayerTest player = new PlayerTest();
            player.name = "Player " + (i+1).ToString();
            player.color = colors[i];
            player.icon = icons[i];
            player.score = 0;
            players.Add(player);
        }
        //Esto habrá que eliminarlo
        //-------------------------

        foreach (PlayerTest player in players)   //Cambiarlo para que se haga por cada jugador que haya en partida (Creo que por los TankDatas)
        {
            GameObject instance = Instantiate(sliderPrefab, panelRT);
            LoadInfo(instance, player);
            sliderScores.Add(instance);
        }
        LeanTween.scale(panelRT, Vector2.zero, 0f);
    }

    private void OnEnable()
    {
        LeanTween.scale(panelRT, Vector2.one, popupTime).setEase(LeanTweenType.easeOutElastic);
        for (int i = 0; i < players.Count; i++)  //numero de jugadores
        {
            players[i].score += UnityEngine.Random.Range(0, 4); //Añadir puntuación
        }
        Invoke("AnimateScores", waitTime);
    }
    #endregion

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
        for(int i=0; i<players.Count; i++)  //numero de jugadores
        {
            AnimateSlider(players[i], sliderScores[i]);
        }
    }

    private void AnimateSlider(PlayerTest player, GameObject slider) //Cambiar a TankData
    {
        SliderLoadInfo sliderInfo = slider.GetComponent<SliderLoadInfo>();
        StartCoroutine(LerpSlider(sliderInfo, sliderInfo.slider.value, player.score, sliderTime));
    }

    private IEnumerator LerpSlider(SliderLoadInfo sliderInfo, float from, float to, float time)
    {
        float i = 0;
        while(i < time)
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
        Invoke("Disable", popupTime);
    }

    private void Disable()
    {
        gameObject.SetActive(false);
    }
}
