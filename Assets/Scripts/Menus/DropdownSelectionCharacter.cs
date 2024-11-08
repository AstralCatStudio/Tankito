using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DropdownSelectionCharacter : MonoBehaviour
{
    public Character character;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private Image characterImage;

    // Start is called before the first frame update
    void Start()
    {
        title.text = character.data.characterName;
        description.text = character.data.description;
        characterImage.sprite = character.data.sprite;
    }
    public void SelectItem()
    {
        if(character.unlocked)
        {
            ClientData.Instance.characterSelected = character;
            Debug.Log("seleccionao");
        }
        else
        {
            Debug.Log("No tienes este personaje todavía");
        }
    }
}
