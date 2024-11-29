using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.UIElements;
namespace Tankito
{
    public class ModifiersUserProfile : MonoBehaviour
    {
        [SerializeField]
        TextMeshProUGUI userName, pointsText;
        [SerializeField]
        GameObject characterImage;
        [SerializeField]
        List<Sprite> modifierImages;
        public void SetUserName(string name)
        {
            userName.text = name;
        }
        public void SetPoints(int points)
        {
            pointsText.text = "Points: " + points;
        }
        public void SetCharacterImage(Sprite sprite)
        {
            characterImage = sprite;
        }
        public void SetModifiers(List<int> modifiers)
        {
            for (int i = 0; i < modifiers.Count; i++)
            {
                modifierImages[i] = ModifierRegistry.Instance.GetModifier(modifiers[i]).GetSprite();
            }
        }
        public void SetModifiers(List<Modifier> modifiers)
        {
            for (int i = 0; i < modifiers.Count; i++)
            {
                modifierImages[i] = modifiers[i].GetSprite();
            }
        }
    }
}
