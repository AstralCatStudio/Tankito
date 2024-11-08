using System.Collections;
using System.Collections.Generic;
using Tankito;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DropdownShopCharacter : MonoBehaviour
{
    public Character character;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private TextMeshProUGUI price;
    [SerializeField] private Image characterImage;

    // Start is called before the first frame update
    void Start()
    {
        title.text = character.data.characterName;
        description.text = character.data.description;
        int number = character.data.price;
        price.text = $"{number}";
        characterImage.sprite = character.data.sprite;
    }
    public void PurchaseItem()
    {
        int price = character.data.price;
        if(ClientData.Instance.money < price)
        {
            Debug.Log("You can't purchase this item");
        } else
        {
            ClientData.Instance.UnlockCharacter(character);
            ClientData.Instance.ChangeMoney(-price);
        }
    }
}
