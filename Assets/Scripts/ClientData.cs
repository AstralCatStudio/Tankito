using System;
using System.Collections;
using System.Collections.Generic;
using Tankito;
using UnityEngine;
using UnityEngine.Events;

public class ClientData : Singleton<ClientData>
{
    [SerializeField]
    private CharacterData[] charactersData;
    public List<Character> characters = new List<Character>();
    public enum accountType
    {
        guest,
        logged
    }
    public int money;  //Moneda del juego (escamas)
    public int skin;   //skin que tenga el jugador puesta. Actualmente es un int pero habrá que hacer que sea una clase o algun tipo para referenciar a la skin
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
        skin = 0;
        username = "guest";
        characters[0].unlocked = true;  // characters[0] -> fish
        characters[0].selected = true;
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
}
