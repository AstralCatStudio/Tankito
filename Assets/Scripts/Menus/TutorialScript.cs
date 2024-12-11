using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialScript : MonoBehaviour
{
    [Header("Parents")]
    [SerializeField] private GameObject tutorialParent;
    [SerializeField] private GameObject circleParent;
    [Header("Prefabs")]
    [SerializeField] private GameObject circlePrefab;
    [SerializeField] private GameObject[] tutorials;
    [Header("Parameters")]
    [SerializeField] private float scaleSize = 1.2f;
    [SerializeField] private float time = 0.5f;
    
    private int currentIndex;
    private List<GameObject> tutorialList = new();
    private List<GameObject> circleList = new();

    private void Start()
    {
        currentIndex = 0;
        InstantiateTutorials();
        tutorialList[currentIndex].SetActive(true);
        ScaleCircle(circleList[currentIndex]);
    }

    private void ScaleCircle(GameObject circle)
    {
        RectTransform rt = circle.GetComponent<RectTransform>();
        LeanTween.scale(rt, Vector2.one * scaleSize, time).setEase(LeanTweenType.easeOutBack);
    }

    private void RescaleCircleToDefault(GameObject circle)
    {
        RectTransform rt = circle.GetComponent<RectTransform>();
        LeanTween.scale(rt, Vector2.one, time).setEase(LeanTweenType.easeOutBack);
    }

    private void InstantiateTutorials()
    {
        foreach(var tutorial in tutorials)
        {
            GameObject instance = Instantiate(tutorial, tutorialParent.transform);
            instance.SetActive(false);
            tutorialList.Add(instance);
        }

        for(int i = 0; i < tutorialList.Count; i++)
        {
            GameObject instance = Instantiate(circlePrefab, circleParent.transform);
            instance.transform.SetSiblingIndex(i+1);
            circleList.Add(instance);
        }

    }

    public void NextTutorial()
    {
        tutorialList[currentIndex].SetActive(false);
        RescaleCircleToDefault(circleList[currentIndex]);
        currentIndex++;
        currentIndex = currentIndex % tutorials.Length;
        tutorialList[currentIndex].SetActive(true);
        ScaleCircle(circleList[currentIndex]);
    }

    public void PreviousTutorial()
    {
        tutorialList[currentIndex].SetActive(false);
        RescaleCircleToDefault(circleList[currentIndex]);
        currentIndex--;
        currentIndex = mod(currentIndex, tutorials.Length); //Custom mod to use negative numbers as well
        tutorialList[currentIndex].SetActive(true);
        ScaleCircle(circleList[currentIndex]);
    }

    int mod(int x, int m)
    {
        int r = x % m;
        return r < 0 ? r + m : r;
    }
}
