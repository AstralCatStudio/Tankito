using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using Tankito;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class ShopContent : MonoBehaviour
{
    private List<GameObject> shopContent = new List<GameObject>();
    [SerializeField]
    private GameObject contentParent;
    [SerializeField]
    private GameObject contentPrefab;

    // Start is called before the first frame update
    void Start()
    {
        CreateShopContent();
    }

    public void CreateShopContent()
    {
        foreach(var character in ClientData.Instance.characters)
        {
            if (character.inShop)
            {
                AddCharacter(character);
            }
        }
    }

    public void AddCharacter(Character character)
    {
        GameObject contentInstance = Instantiate(contentPrefab, contentParent.transform);
        contentInstance.GetComponent<ShopCharacterButton>().character = character;
        shopContent.Add(contentInstance);

        contentInstance.GetComponent<Button>().onClick.AddListener(() => MusicManager.Instance.PlaySound("aceptar"));
    }
}
