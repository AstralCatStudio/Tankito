using System.Collections;
using System.Collections.Generic;
using Tankito;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DropdownCharacter : MonoBehaviour
{
    public CharacterData characterData;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private TextMeshProUGUI price;
    [SerializeField] private Image characterImage;

    // Start is called before the first frame update
    void Start()
    {
        title.text = characterData.characterName;
        description.text = characterData.description;
        int number = characterData.price;
        price.text = $"{number}";
        characterImage.sprite = characterData.sprite;
    }
    public void PurchaseItem()
    {
        int price = characterData.price;
        ClientData clientData = ClientData.Instance;
        clientData.ChangeMoney(-price);
    }
}
