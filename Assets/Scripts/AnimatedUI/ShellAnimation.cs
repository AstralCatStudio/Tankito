using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShellAnimation : MonoBehaviour
{
    [SerializeField] private Sprite shellOpened;
    [SerializeField] private Sprite shellClosed;
    [SerializeField] private GameObject description;
    [SerializeField] private GameObject modifier;
    [SerializeField] private int numOfTurns = 2;
    [SerializeField] private float degrees = 30;
    [SerializeField] private float waitTime = 0.5f;
    [SerializeField] private float animationTime = 0.5f;

    public Action onAnimationFinished;
    
    private void OnEnable()
    {   
        Invoke("StartAnimation", waitTime);
    }

    private void StartAnimation()
    {
        StartCoroutine(AnimateShell());
    }

    private void OnDisable()
    {
        gameObject.GetComponent<Image>().sprite = shellClosed;
        description.SetActive(false);
        modifier.SetActive(false);
    }

    private IEnumerator AnimateShell()
    {
        RectTransform rt = GetComponent<RectTransform>();
        float turn = 0;
        LeanTween.rotateAroundLocal(rt, Vector3.forward, degrees/2, animationTime / (numOfTurns * 2));
        yield return new WaitForSeconds(animationTime / (numOfTurns * 2));
        while ( turn < numOfTurns)
        {
            Debug.Log(turn);
            if(turn % 2 == 0)
            {
                LeanTween.rotateAroundLocal(rt, Vector3.forward, -degrees, animationTime / (numOfTurns * 2));
            } else
            {
                LeanTween.rotateAroundLocal(rt, Vector3.forward, degrees, animationTime / (numOfTurns * 2));
            }
            turn ++ ;
            yield return new WaitForSeconds(animationTime / (numOfTurns * 2));
        }
        LeanTween.rotateAroundLocal(rt, Vector3.forward, -degrees/2, animationTime / (numOfTurns * 2));
        Invoke("Enable", animationTime / 2);
    }

    private void Enable()
    {
        GetComponent<Image>().sprite = shellOpened;
        GetComponent<Button>().enabled = true;
        GetComponent<HoverButton>().enabled = true;
        description.SetActive(true);
        modifier.SetActive(true);
        onAnimationFinished?.Invoke();
    }
}
