using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;
namespace Tankito
{
    public class ModifiersUserProfile : NetworkBehaviour
    {
        [SerializeField]
        TextMeshProUGUI userName, pointsText;
        [SerializeField]
        Image characterImage;
        [SerializeField]
        List<Image> modifierImages;
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
            characterImage.sprite = sprite;
        }
        public void SetModifiers(List<int> modifiers)
        {
            if (modifiers.Count != modifierImages.Count)
            {
                modifierImages.Clear();
                modifierImages = new List<Image>(modifiers.Count);
            }
            for (int i = 0; i < modifiers.Count; i++)
            {
                modifierImages[i].sprite = ModifierRegistry.Instance.GetModifier(modifiers[i]).GetSprite();
            }
        }
        public void SetModifiers(List<Modifier> modifiers)
        {
            if (modifiers.Count != modifierImages.Count)
            {
                modifierImages.Clear();
                modifierImages = new List<Image>(modifiers.Count);
            }
            for (int i = 0; i < modifiers.Count; i++)
            {
                modifierImages[i].sprite = modifiers[i].GetSprite();
            }
        }
    }
}
