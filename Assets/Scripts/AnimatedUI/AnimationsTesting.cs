using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationsTesting : MonoBehaviour
{
    public GameObject scoreScreen;
    public GameObject modifierScreen;
    public GameObject resultsScreen;
    public void Score()
    {
        scoreScreen.SetActive(true);
    }

    public void Modifiers()
    {
        modifierScreen.SetActive(true);
    }
    public void Results()
    {
        resultsScreen.SetActive(true);
    }
}
