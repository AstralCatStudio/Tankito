using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopCharacterButton : MonoBehaviour
{
    [SerializeField] private GameObject dropdown;
    [SerializeField] private float duration = 0.3f;
    public void OpenShopItem()
    {
        var menu = MenuController.Instance;
        var parent = menu.menus[5].transform;
        var dropdownInstance = Instantiate(dropdown, parent);
        var characterPanel = dropdownInstance.transform.GetChild(1).GetComponent<RectTransform>();    //Tiene que coger el hijo que sea el CharacterPanel
        LeanTween.scale(characterPanel, Vector2.zero, 0f);
        LeanTween.scale(characterPanel, Vector2.one, duration).setEase(LeanTweenType.easeOutElastic);
    }
}
