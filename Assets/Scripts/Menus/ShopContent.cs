using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using Tankito;
using UnityEngine;

public class ShopContent : MonoBehaviour
{
    [SerializeField]
    private List<CharacterData> characters = new List<CharacterData>(); //los datos de todos los personajes disponibles
    private List<CharacterData> chosenCharacters = new List<CharacterData>();  //los datos que la tienda escoge (ahora mismo solo se hace cada vez que se hace Start, pero habría que hacer que se elija el contenido en base a una semilla que cambia cada dia)
    private List<GameObject> shopContent = new List<GameObject>();
    [SerializeField]
    private GameObject contentParent;
    [SerializeField]
    private GameObject contentPrefab;
    [SerializeField]
    private int numContent; //Número de contenido disponible en la tienda

    // Start is called before the first frame update
    void Start()
    {
        while(chosenCharacters.Count < numContent)
        {
            CharacterData dataPicked = characters[Random.Range(0, characters.Count)];
            if(!chosenCharacters.Contains(dataPicked))
            {
                chosenCharacters.Add(dataPicked);
                GameObject contentInstance = Instantiate(contentPrefab, contentParent.transform);
                contentInstance.GetComponent<ShopCharacterButton>().characterData = dataPicked;
                shopContent.Add(contentInstance);
            }
        }

        foreach(CharacterData dataPicked in chosenCharacters)
        {
            Debug.Log(dataPicked.name.ToString());
        }
    }
}
