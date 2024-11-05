using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "CharacterData", menuName = "Characters", order = 0)]
public class CharacterData : ScriptableObject
{
    public Sprite sprite;
    public string Name;
    [TextArea(5,10)]
    public string Description;
    public int price;
}
