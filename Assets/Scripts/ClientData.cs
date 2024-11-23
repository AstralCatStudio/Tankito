using System;
using System.Collections;
using System.Collections.Generic;
using Tankito;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ClientData : Singleton<ClientData>
{
    [SerializeField]
    private CharacterData[] charactersData;
    public List<Character> characters = new List<Character>();
    //Assets to update when a skin is selected
    [SerializeField] private Image selectionSkinButton;

    public enum accountType
    {
        guest,
        logged
    }
    public int money;  //Moneda del juego (escamas)
    public int numShopContent;
    public string username;
    public accountType accountT;   //Tipo de la cuenta al iniciar sesión
    public event Action onMoneyChanged, onUsernameChanged, onCharacterPurchased;

    public bool firstLoad = true;

    // Start is called before the first frame update
    void Start()
    {
        InitCharactersData();
        InitClientData();
        ChooseCharactersInShop();

        firstLoad = true;

        DontDestroyOnLoad(gameObject);
    }

    private void InitClientData()
    {
        money = 0;
        accountT = 0;
        username = "guest";
        characters[0].unlocked = true;  //characters[0] -> fish
    }

    public void InitSelectCharacter()
    {
        Scene loadedScene = SceneManager.GetSceneByName("MainMenu");
        if (loadedScene.IsValid() && loadedScene.isLoaded)
        {
            foreach (GameObject rootObject in loadedScene.GetRootGameObjects())
            {
                //Debug.Log(rootObject.name);
                if(rootObject.name == "Canvas")
                {
                    //Debug.Log("Canvas encontrado");

                    Transform btCharacterSelection = rootObject.transform.Find("PlayMenu/BT_CharacterSelection");

                    if(btCharacterSelection != null)
                    {
                        //Debug.Log("Encontrado");
                        selectionSkinButton = btCharacterSelection.GetComponent<Image>();
                        if(firstLoad)
                        {
                            SelectCharacter(characters[0]);
                        }
                        else
                        {
                            SelectCharacter(GetCharacterSelected());
                        }
                        
                    }
                    else
                    {
                        Debug.LogWarning("Selección de personaje no encontrada");
                    }

                    return;
                }
            }
        }
    }

    private void InitCharactersData()
    {
        foreach (var cData in charactersData)
        {
            Character newCharacter = new Character();
            newCharacter.data = cData;
            newCharacter.unlocked = false;
            characters.Add(newCharacter);
        }
    }

    private void ChooseCharactersInShop()
    {
        int i = 0;
        while (i < numShopContent)
        {
            Character characterPicked = characters[UnityEngine.Random.Range(1, characters.Count)];  //Pilla desde el 1, porque el 0 es el Pez
            if (!characterPicked.inShop)
            {
                Debug.Log(characterPicked.data.characterName.ToString());
                characterPicked.inShop = true;
                i++;
            }
        }
    }
    public void ChangeMoney(int moneyAdded)
    {
        money += moneyAdded;
        money = Mathf.Clamp(money, 0, 999999999);
        onMoneyChanged?.Invoke();
    }

    public void ChangeUsername(string newUsername)
    {
        username = newUsername;
        onUsernameChanged?.Invoke();
    }

    public void UnlockCharacter(Character character)
    {
        character.unlocked = true;
        onCharacterPurchased?.Invoke();
    }

    public void SelectCharacter(Character character)
    {
        foreach (var c in characters)
        {
            c.selected = false;
        }
        character.selected = true;
        selectionSkinButton.sprite = character.data.sprite;
    }
    public Character GetCharacterSelected()
    {
        foreach (var c in characters)
        {
            if (c.selected) return c;
        }
        return characters[0];
    }
}
