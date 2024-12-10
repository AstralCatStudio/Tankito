using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoverButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float scale = 1.2f;
    public float time= 0.5f;
    private LTDescr tweenId;
    private Color color;
    private float hoverColorFactor = 1f;
    // Start is called before the first frame update
    void Start()
    {
        color = GetComponent<Image>().color;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        tweenId = LeanTween.scale(gameObject, Vector2.one * scale, time).setEase(LeanTweenType.easeOutElastic);
        Color newColor = color * hoverColorFactor;
        newColor.a = 1f;
        gameObject.GetComponent<Image>().color = newColor;
        MusicManager.Instance.PlaySoundPitch("bip",0.1f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        LeanTween.cancel(gameObject, tweenId.id);
        LeanTween.scale(gameObject, Vector2.one, time).setEase(LeanTweenType.easeOutCubic);
        gameObject.GetComponent<Image>().color = color;        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
