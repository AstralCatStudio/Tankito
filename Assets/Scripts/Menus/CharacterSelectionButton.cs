using System.Collections;
using System.Collections.Generic;
using Tankito;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectionButton : MonoBehaviour
{
    //Character Data
    public Character character;
    //ButtonMedia
    [SerializeField] private GameObject locked;
    [SerializeField] private GameObject selected;
    [SerializeField] private TextMeshProUGUI textName;
    [SerializeField] private Image characterImage;
    //Animation Data
    [SerializeField] private GameObject dropdown;
    [SerializeField] private float duration = 0.3f;

    private void Start()
    {
        characterImage.sprite = character.data.sprite;
        textName.text = character.data.characterName;

        ClientData.Instance.onCharacterSelected += SelectCharacter;

        if (character.unlocked)
        {
            if(character.selected)
            {
                selected.SetActive(true); 
            }
            locked.SetActive(false);
        } else
        {
            ClientData.Instance.onCharacterPurchased += UnlockCharacter;
        }
    }
    public void OpenCharacterSelectItem()
    {
        var menu = MenuController.Instance;
        var parent = menu.menus[(int)MenuController.menuName.CharacterSelection].transform;
        var dropdownInstance = Instantiate(dropdown, parent);
        dropdownInstance.GetComponent<DropdownSelectionCharacter>().character = character;
        var characterPanel = dropdownInstance.transform.GetChild(1).GetComponent<RectTransform>();    //Tiene que coger el hijo que sea el CharacterPanel
        LeanTween.scale(characterPanel, Vector2.zero, 0f);
        LeanTween.scale(characterPanel, Vector2.one, duration).setEase(LeanTweenType.easeOutElastic);

        MusicManager.Instance.PlaySound("aceptar");
    }

    private void UnlockCharacter()
    {
        if (character.unlocked)
        {
            locked.SetActive(false);
            ClientData.Instance.onCharacterPurchased -= UnlockCharacter;
        }
    }
    private void SelectCharacter()
    {
        selected.SetActive(false);
        if(character.selected)
            selected.SetActive(true);
    }
}
