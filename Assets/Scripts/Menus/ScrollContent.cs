using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollContent : MonoBehaviour
{
    [SerializeField] private RectTransform contentPanel;
    [SerializeField] private int widthToScroll = 360;
    [SerializeField] private float time = 0.1f;
    public void moveToRight()
    {
        Vector2 scrollPos = contentPanel.anchoredPosition;
        scrollPos.x += widthToScroll;
        LeanTween.move(contentPanel, scrollPos, time).setEase(LeanTweenType.easeOutCubic);
    }

    public void moveToLeft()
    {
        Vector2 scrollPos = contentPanel.anchoredPosition;
        scrollPos.x -= widthToScroll;
        LeanTween.move(contentPanel, scrollPos, time).setEase(LeanTweenType.easeOutCubic);
    }
}
