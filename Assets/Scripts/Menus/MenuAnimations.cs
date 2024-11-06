using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Multiplayer.Tools.NetStatsMonitor;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

public class MenuAnimations : MonoBehaviour
{
    private List<RectTransform> uiElements = new();
    private List<Vector2> originalRTransforms = new();
    private List<Vector2> newPositions = new();
    [SerializeField] private float enterDuration = 1f;
    [SerializeField] private float exitDuration = 1f;
    [SerializeField] private float enableTime = 1f;
    [SerializeField] private float disableTime = 1f;
    [SerializeField] private float delay = 0.1f;

    private Coroutine disableCoroutine, enablingInteractable;

    private void Awake()
    {
        int childCount = gameObject.transform.childCount;
        for(int i=0; i<childCount; i++)
        {
            uiElements.Add(gameObject.transform.GetChild(i).GetComponent<RectTransform>());
            originalRTransforms.Add(uiElements[i].anchoredPosition);
            if (uiElements[i].transform.GetComponent<Button>() == true)
            {
                uiElements[i].transform.GetComponent<Button>().interactable = false;
            }
        }

        if(uiElements != null)
        {
            foreach (RectTransform element in uiElements)
            {
                Vector2 newPosition = CalculateNewPosition(element);
                newPositions.Add(newPosition);
                element.anchoredPosition = newPosition;
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
    private void EnableDelayed()
    {
        EnablingAnimation();
    }

    public void EnablingAnimation()
    {
        float additiveDelay = delay;
        for (int i = 0; i < uiElements.Count; i++)
        {
            LeanTween.move(uiElements[i], originalRTransforms[i], enterDuration).setEase(LeanTweenType.easeOutBack).setDelay(additiveDelay);
            additiveDelay += delay;
        }

        Debug.Log("pre enabling routine");
        if (enablingInteractable != null)
        {
            StopCoroutine(enablingInteractable);
        }
        enablingInteractable = StartCoroutine(EnableInteractables(enterDuration));
    }

    public void DisablingAnimation()
    {
        float additiveDelay = delay;
        for (int i = 0; i < uiElements.Count; i++)
        {
            if (uiElements[i].transform.GetComponent<Button>() == true)
            {
                uiElements[i].transform.GetComponent<Button>().interactable = false;
            }

            LeanTween.move(uiElements[i], newPositions[i], exitDuration).setEase(LeanTweenType.easeInBack).setDelay(additiveDelay);
            additiveDelay += delay;
        }

        Debug.Log("A.1");

        if (disableCoroutine != null)
        {
            Debug.Log("A.2");
            StopCoroutine(disableCoroutine);
        }
        disableCoroutine = StartCoroutine(DisablingCoroutine(disableTime));
    }

    IEnumerator DisablingCoroutine(float disableTime)
    {
        float t = 0;
        while(t < disableTime)
        {
            yield return null;
            t += Time.deltaTime;
        }
        Debug.Log("desactivao");
        gameObject.SetActive(false);
    }

    IEnumerator EnableInteractables(float enterTransition)
    {
        float t = 0;
        while (t < enterTransition)
        {
            yield return null;
            t += Time.deltaTime;
        }
        for (int i = 0; i < uiElements.Count; i++)
        {
            if(uiElements[i].transform.GetComponent<Button>() == true)
            {
                uiElements[i].transform.GetComponent<Button>().interactable = true;
            }
        }
    }
}
