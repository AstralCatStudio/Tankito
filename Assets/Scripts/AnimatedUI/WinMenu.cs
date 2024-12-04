using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class WinMenu : MonoBehaviour
{
    public int m_num_players = 0;
    public int m_max_players = 4;

    [SerializeField] private GameObject prefabSprite;
    [SerializeField] private GameObject prefabMarcador;
    [SerializeField] private GameObject prefabPuesto;

    [SerializeField] private GameObject spritesParent;
    [SerializeField] private GameObject marcadoresParent;
    [SerializeField] private GameObject puestosParent;

    [SerializeField] private List<GameObject> spritesList;
    [SerializeField] private List<GameObject> marcadoresList;
    [SerializeField] private List<GameObject> puestosList;

    public float offsetY;

    public float animDuration = 0.5f;
    public float scaleDuration = 0.3f;

    private void Awake()
    {
        spritesList = new List<GameObject>();
        marcadoresList = new List<GameObject>();
        puestosList = new List<GameObject>();
    }

    void Start()
    {
        InitWinMenu(m_num_players);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Inicio animacion");
            StartCoroutine(AnimateSprites());
        }
    }

    private void OnEnable()
    {
        //StartCoroutine(AnimateSprites());
    }

    public void InitWinMenu(int playersCount)
    {
        for (int i = 0; i < playersCount; i++)
        {
            GameObject newSprite = Instantiate(prefabSprite, spritesParent.transform);
            newSprite.transform.GetChild(0).gameObject.SetActive(false);
            spritesList.Add(newSprite);
            GameObject newMarcador = Instantiate(prefabMarcador, marcadoresParent.transform);
            newMarcador.transform.GetChild(0).gameObject.SetActive(false);
            marcadoresList.Add(newMarcador);
            GameObject newPuesto = Instantiate(prefabPuesto, puestosParent.transform);
            newPuesto.transform.GetChild(0).gameObject.SetActive(false);
            puestosList.Add(newPuesto);
        }
    }

    IEnumerator AnimateSprites()
    {
        string[] soundNames = { "WinTankitoSounds4", "WinTankitoSounds3", "WinTankitoSounds2", "WinTankitoSounds1" };

        for (int i = m_num_players - 1; i >= 0; i--)
        {
            RectTransform sprite = spritesList[i].transform.GetChild(0).GetComponent<RectTransform>();
            RectTransform marcador = marcadoresList[i].transform.GetChild(0).GetComponent<RectTransform>();
            RectTransform puesto = puestosList[i].transform.GetChild(0).GetComponent<RectTransform>();

            sprite.gameObject.SetActive(true);
            LeanTween.moveY(sprite, offsetY, animDuration).setEase(LeanTweenType.easeOutBounce);


            puesto.localScale = Vector3.zero;
            puesto.gameObject.SetActive(true);
            LeanTween.scale(puesto, Vector3.one, scaleDuration).setEase(LeanTweenType.easeOutBack);

            MusicManager.Instance.PlaySound(soundNames[i]);

            yield return new WaitForSeconds(animDuration);

            marcador.localScale = Vector3.zero;
            marcador.gameObject.SetActive(true);
            LeanTween.scale(marcador, Vector3.one, scaleDuration).setEase(LeanTweenType.easeOutBack);
        }
    }
}
