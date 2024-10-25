using System.Collections;
using System.Collections.Generic;
using Unity.Multiplayer.Tools.NetStatsMonitor;
using UnityEngine;
using static UnityEditor.Rendering.FilterWindow;

public class MenuAnimations : MonoBehaviour
{
    [SerializeField] private List<RectTransform> uiElements;
    [SerializeField] private List<Vector2> originalRTransforms;
    [SerializeField] private List<Vector2> newPositions;
    [SerializeField] private float enterDuration = 1f;
    [SerializeField] private float exitDuration = 1f;
    [SerializeField] private float enableTime = 1f;
    [SerializeField] private float disableTime = 1f;
    [SerializeField] private float delay = 0.1f;

    private void Awake()
    {
        int childCount = gameObject.transform.childCount;
        for(int i=0; i<childCount; i++)
        {
            uiElements.Add(gameObject.transform.GetChild(i).GetComponent<RectTransform>());
            originalRTransforms.Add(uiElements[i].anchoredPosition);
        }

        if(uiElements != null)
        {
            foreach (RectTransform element in uiElements)
            {
                Vector2 newPosition = CalculateNewPosition(element);
                newPositions.Add(newPosition);
                LeanTween.move(element, newPosition, 0f);
            }
        }
    }

    private Vector2 CalculateNewPosition(RectTransform element)
    {
        Vector3 anchor = element.anchorMax;
        anchor.x *= 2; anchor.y *= 2;
        anchor.x -= 1; anchor.y -= 1;
        Vector2 newPosition;
        if (anchor.x == 0 && anchor.y == 0)
        {
            var signx = Mathf.Sign(element.anchoredPosition.x);
            var signy = Mathf.Sign(element.anchoredPosition.y);
            newPosition = element.anchoredPosition + new Vector2(signx * ((Screen.width) - (Mathf.Abs(element.anchoredPosition.x) + element.sizeDelta.x)), signy * ((Screen.height) - (Mathf.Abs(element.anchoredPosition.y) + element.sizeDelta.y)));
        }
        else
        {
            newPosition = element.anchoredPosition + new Vector2(anchor.x * Mathf.Abs(element.anchoredPosition.x) + anchor.x * element.sizeDelta.x, anchor.y * Mathf.Abs(element.anchoredPosition.y) + anchor.y * element.sizeDelta.y);
        }
        return newPosition;
    }

    private void OnEnable()
    {
        Invoke("EnableDelayed", enableTime);
    }
    public void EnablingAnimation()
    {
        float additiveDelay = delay;
        for (int i = 0; i < uiElements.Count; i++)
        {
            LeanTween.move(uiElements[i], originalRTransforms[i], enterDuration).setEase(LeanTweenType.easeOutBack).setDelay(additiveDelay);
            additiveDelay += delay;
        }
    }

    public void DisablingAnimation()
    {
        float additiveDelay = delay;
        for (int i = 0; i < uiElements.Count; i++)
        {
            LeanTween.move(uiElements[i], newPositions[i], exitDuration).setEase(LeanTweenType.easeInBack).setDelay(additiveDelay);
            additiveDelay += delay;
        }

        Invoke("DisableDelayed", disableTime);
    }

    private void EnableDelayed()
    {
        EnablingAnimation();
    }

    private void DisableDelayed()
    {
        gameObject.SetActive(false);
    }
}
