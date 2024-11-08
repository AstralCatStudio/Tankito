using System.Collections;
using System.Collections.Generic;
using Tankito;
using UnityEngine;

public class CharacterSelection : MonoBehaviour
{
    private List<GameObject> characterSelectionContent = new List<GameObject>();
    [SerializeField]
    private GameObject contentParent;
    [SerializeField]
    private GameObject contentPrefab;
    // Start is called before the first frame update
    void Start()
    {
        var characters = ClientData.Instance.characters;
        foreach(var character in characters)
        {
            GameObject contentInstance = Instantiate(contentPrefab, contentParent.transform);
            contentInstance.GetComponent<CharacterSelectionButton>().character = character;
            characterSelectionContent.Add(contentInstance);
        }
    }
}
