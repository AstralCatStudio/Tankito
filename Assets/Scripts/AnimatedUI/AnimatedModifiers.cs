using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimatedModifiers : MonoBehaviour
{
    [Header("Params")]
    #region parameters
    [SerializeField] private float popupTime = 0.5f;
    [SerializeField] private float waitTime = 0.5f;
    [SerializeField] private float shellTime = 0.5f;
    [SerializeField, Range(2, 4)] private int numPlayers = 2;
    [SerializeField] private RectTransform panelRT;
    [SerializeField] private GameObject shellPrefab;
    [SerializeField] private Transform row1;
    [SerializeField] private Transform row2;
    [SerializeField] private List<GameObject> shells;
    #endregion

    #region removeThis
    [Header("Remove this params")]
    [SerializeField] private List<PlayerTest> players = new();  //Esto se podrá eliminar
    private Color[] colors = { Color.blue, Color.red, Color.green, Color.yellow };
    [SerializeField] private Sprite[] icons;
    #endregion

    #region UnityFunctions
    private void Awake()
    {
        //-------------------------
        //Esto habrá que eliminarlo
        for (int i = 0; i < numPlayers; i++)
        {
            PlayerTest player = new PlayerTest();
            player.name = "Player " + (i + 1).ToString();
            player.color = colors[i];
            player.icon = icons[i];
            player.score = 0;
            players.Add(player);
        }
        //Esto habrá que eliminarlo
        //-------------------------

        for(int i=0; i < numPlayers + 1; i++) //Cambiarlo para que se haga por cada jugador que haya en partida (Creo que por los TankDatas)
        {
            GameObject instance;
            if(i/3 == 0)
            {
                instance = Instantiate(shellPrefab, row1);
            } else if(i / 3 == 1){
                instance = Instantiate(shellPrefab, row2);
            } else
            {
                instance = null;
            }

            if(instance != null)
                shells.Add(instance);
        }

        LeanTween.scale(panelRT, Vector2.zero, 0f);
    }

    private void OnEnable()
    {
        LeanTween.scale(panelRT, Vector2.one, popupTime).setEase(LeanTweenType.easeOutElastic);
        //for (int i = 0; i < players.Count; i++)  //numero de jugadores
        //{
        //    players[i].score += Random.Range(0, 4); //Añadir puntuación
        //}
        //Invoke("AnimateShells", waitTime);
    }
    #endregion

    public void Disappear()
    {
        LeanTween.scale(panelRT, Vector2.zero, popupTime).setEase(LeanTweenType.easeInBack);
        Invoke("Disable", popupTime);
    }

    private void Disable()
    {
        gameObject.SetActive(false);
    }
}
