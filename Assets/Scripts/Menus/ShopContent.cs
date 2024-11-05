using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopContent : MonoBehaviour
{
    [SerializeField]
    private List<CharacterData> characters = new List<CharacterData>();
    [SerializeField]
    private List<CharacterData> shopContent = new List<CharacterData>();
    [SerializeField]
    private int numContent;

    // Start is called before the first frame update
    void Start()
    {
        for(int i=0; i<numContent; i++)
        {
            //Hacer que se creen un numero de paneles con todos los datos de los personajes
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
