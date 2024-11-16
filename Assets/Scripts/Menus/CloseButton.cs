using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseButton : MonoBehaviour
{
    [SerializeField] private GameObject dropdown;
    [SerializeField] private float duration = 0.5f;
    public void CloseDropdown()
    {
        var bg = dropdown.transform.GetChild(0).GetComponent<RectTransform>();    //Tiene que coger el hijo que sea el Bg
        var characterPanel = dropdown.transform.GetChild(1).GetComponent<RectTransform>();    //Tiene que coger el hijo que sea el CharacterPanel
        LeanTween.alpha(bg, 0f, duration);
        LeanTween.scale(characterPanel, Vector2.zero, duration).setEase(LeanTweenType.easeInBack);
        Invoke("DestroyDropdown", duration);

        MusicManager.Instance.PlaySound("aceptar");
    }

    private void DestroyDropdown()
    {
        Destroy(dropdown);
    }
}
