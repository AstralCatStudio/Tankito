using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
namespace Tankito
{
    public class ModifierSelector : MonoBehaviour
    {
        [SerializeField]
        TextMeshProUGUI title;
        [SerializeField]
        TextMeshProUGUI description;
        [SerializeField]
        Image image;
        [SerializeField]
        Modifier modifier;
        public Action<ModifierSelector> ChooseModifier = (modifier) =>{};
        public bool available = true;
        public void ApplyModifier(Modifier newModifier)
        {
            modifier = newModifier;
            title.text = ModifierRegistry.Instance.GetModifierTitle(modifier);
            description.text = ModifierRegistry.Instance.GetModifierDescription(modifier);
            image.sprite = ModifierRegistry.Instance.GetModifierIcon(modifier);
        }
        Modifier GetModifier()
        {
            return modifier;
        }
        public void SelectModifier()
        {
            ChooseModifier(this);
        }
    }
}
