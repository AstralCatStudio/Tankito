using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuTitleInit : MonoBehaviour
{
    [SerializeField] private float inDuration = 2f;
    [SerializeField] private float outDuration = 2f;
    [SerializeField] private GameObject firstMenu;

    // Start is called before the first frame update
    void Start()
    {
        RectTransform ltRect = gameObject.GetComponent<RectTransform>();
        //0
        LeanTween.alpha(ltRect, 0f, 0f);
        //FadeIn
        LeanTween.alpha(ltRect, 1f, inDuration).setEase(LeanTweenType.easeInSine);
        //FadeOut
        LeanTween.alpha(ltRect, 0f, outDuration).setEase(LeanTweenType.easeOutSine).setDelay(inDuration);

        Invoke("EnableDelayed", inDuration + outDuration);
    }

    private void EnableDelayed()
    {
        firstMenu.SetActive(true);
    }
}
