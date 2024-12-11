using System.Collections;
using System.Collections.Generic;
using Tankito;
using UnityEngine;

public class DisplayTutorial : MonoBehaviour
{
    [SerializeField] private GameObject dropdownTutorial;
    [SerializeField] private float duration = 0.3f;
    
    public void OpenTutorial()
    {
        var menu = MenuController.Instance;
        var parent = transform.parent;
        var dropdownInstance = Instantiate(dropdownTutorial, parent);
        var tutorialPanel = dropdownInstance.transform.GetChild(1).GetComponent<RectTransform>();
        LeanTween.scale(tutorialPanel, Vector2.zero, 0f);
        LeanTween.scale(tutorialPanel, Vector2.one, duration).setEase(LeanTweenType.easeOutElastic);

        MusicManager.Instance.PlaySound("aceptar");
    }
}
