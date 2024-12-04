using System.Collections;
using UnityEngine;

public class WinMenu : MonoBehaviour
{
    public int m_num_players = 0;
    public int m_max_players = 4;

    public GameObject marcadorJugador1;
    public GameObject marcadorJugador2;
    public GameObject marcadorJugador3;
    public GameObject marcadorJugador4;

    public GameObject spriteJugador1;
    public GameObject spriteJugador2;
    public GameObject spriteJugador3;
    public GameObject spriteJugador4;

    public GameObject puesto1;
    public GameObject puesto2;
    public GameObject puesto3;
    public GameObject puesto4;

    public GameObject exitButton;

    public float offsetY = 50f;

    public float animDuration = 0.5f;
    public float scaleDuration = 0.3f;

    void Start()
    {
    }

    private void OnEnable()
    {
        StartCoroutine(AnimateSprites());
    }

    IEnumerator AnimateSprites()
    {
        GameObject[] sprites = { spriteJugador4, spriteJugador3, spriteJugador2, spriteJugador1 };
        GameObject[] marcadores = { marcadorJugador4, marcadorJugador3, marcadorJugador2, marcadorJugador1 };
        GameObject[] puestos = { puesto4, puesto3, puesto2, puesto1 };
        string[] soundNames = { "WinTankitoSounds1", "WinTankitoSounds2", "WinTankitoSounds3", "WinTankitoSounds4" };

        for (int i = m_max_players - m_num_players; i < m_max_players; i++)
        {
            GameObject sprite = sprites[i];
            GameObject marcador = marcadores[i];
            GameObject puesto = puestos[i];


            RectTransform marcadorTransform = marcador.GetComponent<RectTransform>();
            RectTransform spriteTransform = sprite.GetComponent<RectTransform>();
            
            float targetY = offsetY;

            sprite.SetActive(true);
            
            LeanTween.moveY(spriteTransform, targetY, animDuration).setEase(LeanTweenType.easeOutBounce);

            puesto.SetActive(true);
            RectTransform puestoTransform = puesto.GetComponent<RectTransform>();
            puestoTransform.localScale = Vector3.zero;
            LeanTween.scale(puestoTransform, Vector3.one, scaleDuration).setEase(LeanTweenType.easeOutBack);

            MusicManager.Instance.PlaySound(soundNames[i]);

            yield return new WaitForSeconds(animDuration);

            marcador.SetActive(true);
            RectTransform marcadorRectTransform = marcador.GetComponent<RectTransform>();
            marcadorRectTransform.localScale = Vector3.zero;
            LeanTween.scale(marcadorRectTransform, Vector3.one, scaleDuration).setEase(LeanTweenType.easeOutBack);
        }


        exitButton.SetActive(true);
        RectTransform exitButtonTransform = exitButton.GetComponent<RectTransform>();
        exitButtonTransform.localScale = Vector3.zero;
        LeanTween.scale(exitButtonTransform, Vector3.one, scaleDuration).setEase(LeanTweenType.easeOutBack);
    }
}
