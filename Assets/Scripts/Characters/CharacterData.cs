using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Characters", order = 0)]
public class CharacterData : ScriptableObject
{
    public Sprite sprite;
    public Sprite tankHead;
    public Sprite tankBody;
    public Sprite characterIcon;
    public Sprite fishSprite;
    public RuntimeAnimatorController fishAnimator;
    public string characterName;
    [TextArea(5,10)]
    public string description;
    public int price;
}
