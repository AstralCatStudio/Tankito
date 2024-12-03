using System;
using System.Collections;
using System.Collections.Generic;
using Tankito;
using UnityEngine;
using UnityEngine.UI;

public class AnimatedModifiers : MonoBehaviour
{
    [Header("Params")]
    #region parameters
    [SerializeField, Range(0, 2)] private float popupTime = 0.5f;
    [SerializeField, Range(0, 2)] private float waitTime = 0.5f;
    [SerializeField, Range(0, 2)] private float shellTime = 0.5f;
    [SerializeField, Range(0, 2)] private float playerTransitionTime = 0.5f;
    [SerializeField, Range(0, 2)] private float playerScale = 1.5f;
    [SerializeField, Range(2, 4)] private int numPlayers = 2;
    private int turn = -1;

    [SerializeField] private RectTransform panelRT;
    [SerializeField] private GameObject shellPrefab;
    
    [SerializeField] private RectTransform playerChoosingPosition;
    [SerializeField] private GameObject otherPlayerPrefab;
    [SerializeField] private GameObject otherPlayersPanel;

    [SerializeField] private Transform row1;
    [SerializeField] private Transform row2;
    [SerializeField] private List<GameObject> shells;

    public ModifierList modifierList;
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

        //Instancia los objetos de los modificadores
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
            instance.GetComponent<Button>().enabled = false;
            instance.GetComponent<HoverButton>().enabled = false;
        }

        //Para testear las puntuaciones
        foreach(PlayerTest player in players)
        {
            player.score += UnityEngine.Random.Range(0, 4);
        }

        //Instancia los objetos donde irá la información del resto de jugadores
        foreach (PlayerTest player in players)
        {
            GameObject instance;
            instance = Instantiate(otherPlayerPrefab, otherPlayersPanel.transform);
            player.playerInfo = instance;
            OtherPlayersLoadInfo(player);
        }

        LeanTween.scale(panelRT, Vector2.zero, 0f);
    }

    private void OnEnable()
    {
        //Para testear las puntuaciones
        foreach (PlayerTest player in players)
        {
            player.score += UnityEngine.Random.Range(0, 4);
        }

        SortPlayers();
            
        foreach(PlayerTest player in players)
        {
            UpdateValues(player);
        }

            LeanTween.scale(panelRT, Vector2.one, popupTime).setEase(LeanTweenType.easeOutElastic);
        shells[0].GetComponent<ShellAnimation>().onAnimationFinished += StartChoose;
        
        //for (int i = 0; i < players.Count; i++)  //numero de jugadores
        //{
        //    players[i].score += Random.Range(0, 4); //Añadir puntuación
        //}
    }
    #endregion

    private void OtherPlayersLoadInfo(PlayerTest player)
    {
        OtherP_LoadInfo otherP = player.playerInfo.GetComponent<OtherP_LoadInfo>();
        otherP.icon.sprite = player.icon;
        otherP.name.text = player.name;
        otherP.name.color = player.color;
        otherP.score.text = player.score.ToString();
        otherP.position.text = player.position.ToString() + ".";
    }

    private void UpdateValues(PlayerTest player)
    {
        OtherP_LoadInfo otherP = player.playerInfo.GetComponent<OtherP_LoadInfo>();
        otherP.score.text = player.score.ToString();
        otherP.position.text = player.position.ToString() + ".";
    }

    private void SortPlayers()
    {
        players.Sort();

        for(int i = 0; i < numPlayers; i++)
        {
            players[i].playerInfo.transform.SetSiblingIndex((numPlayers - 1) - i);
        }

        //Pone las posiciones de los jugadores, y se asegura de que, si dos tienen la misma puntuación, están en la misma posicion
        for (int i = 0; i < numPlayers; i++)
        {
            int position = i + 1;
            players[i].position = position;
            while (i + 1 < numPlayers && players[i].score == players[i + 1].score)
            {
                players[i + 1].position = position;
                i++;
            }
        }
    }

    private void StartChoose()
    {
        turn = numPlayers - 1;
        AnimatePlayer(players[turn]);
    }

    private void AnimatePlayer(PlayerTest player)
    {
        RectTransform playerRT = player.playerInfo.GetComponent<RectTransform>();
        player.playerInfo.transform.SetParent(playerChoosingPosition);
        LeanTween.move(playerRT, playerChoosingPosition.anchoredPosition, playerTransitionTime).setEase(LeanTweenType.easeInOutCubic);
        LeanTween.scale(playerRT, Vector3.one * playerScale, playerTransitionTime).setEase(LeanTweenType.easeInOutCubic);

        ShellAnimation shellAnimation;
        //Para que los modificadores puedan ser seleccionables, llamamos a Enable
        foreach(GameObject shell in shells)
        {
            shellAnimation = shell.GetComponent<ShellAnimation>();
            shellAnimation.Enable();
        }
    }
    
    public void NextTurn()
    {
        GameObject shellSelected = CheckSelected();
        if (shellSelected == null)
        {
            Debug.LogWarning("You didn´t choose any modifier");
        } else
        {
            shellSelected.GetComponent<ShellAnimation>().Disable();
        }
    }
    private GameObject CheckSelected()
    {
        ShellSelection selection;
        foreach(GameObject shell in shells)
        {
            selection = shell.GetComponent<ShellSelection>();
            if (selection.selected)
            {
                return shell;
            }
        }
        return null;
    }

    public void DeselectAllModifiers()
    {
        foreach(GameObject s in shells)
        {
            s.GetComponent<ShellSelection>().selected = false;
            s.GetComponent<Outline>().enabled = false;
        }
    }

    public void Disappear()
    {
        LeanTween.scale(panelRT, Vector2.zero, popupTime).setEase(LeanTweenType.easeInBack);
        DeselectAllModifiers();
        foreach(GameObject s in shells)
        {
            s.GetComponent<Button>().enabled = false;
            s.GetComponent<HoverButton>().enabled = false;
        }
        Invoke("Disable", popupTime);
    }

    private void Disable()
    {
        gameObject.SetActive(false);
        shells[0].GetComponent<ShellAnimation>().onAnimationFinished -= StartChoose;
    }

    
}
