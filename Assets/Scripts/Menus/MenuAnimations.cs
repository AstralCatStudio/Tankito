using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuAnimations : MonoBehaviour
{
    [SerializeField] private List<RectTransform> uiElements;
    [SerializeField] private List<Vector2> originalRTransforms;
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
                LeanTween.move(element, new Vector3(Screen.width * 2,Screen.height * 2f), 0f);
            }
        }
        
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
        Vector3 newPosition;
        for (int i = 0; i < uiElements.Count; i++)
        {
            newPosition = uiElements[i].transform.position + new Vector3(Screen.width, Screen.height);
            LeanTween.move(uiElements[i], newPosition, exitDuration).setEase(LeanTweenType.easeInBack).setDelay(additiveDelay);
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
