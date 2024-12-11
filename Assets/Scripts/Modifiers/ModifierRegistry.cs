using System.Collections.Generic;
using UnityEngine;

namespace Tankito
{
    public enum ModifierName //si se añaden modificadores añadirlos a este enum con el index en la lista de modificadores o no, ya no hace falta esto creo
    {
        Shotgun,
        NoFriendlyFire,
        FasterTank,
        FasterBullets,
        MoreBullets,
        LongerParry,
        LongerDash,
        LessReloadTime,
        LowerParryCooldown,
        LowerDashCooldown,
        FasterDash,
        OppositeBullet,
        ExplosiveBullets,
        BouncyBullets,
        BigBullets
    }
    public class ModifierRegistry : Singleton<ModifierRegistry>
    {
        [SerializeField]
        ModifierList modifierList;
        public Modifier GetModifier(ModifierName modifierID)
        {
            return modifierList.GetModifier((int)modifierID);
        }
        public Sprite GetModifierIcon(ModifierName modifierID)
        {
            return GetModifier(modifierID).GetSprite();
        }
        public string GetModifierDescription(ModifierName modifierID)
        {
            return GetModifier(modifierID).GetDescription();
        }
        public string GetModifierTitle(ModifierName modifierID)
        {
            return GetModifier(modifierID).GetTitle();
        }
        public Modifier GetRandomModifier()
        {
            int randomModifier = Random.Range(0, modifierList.modifiers.Count);
            return GetModifier(randomModifier);
        }
        public List<Modifier> GetRandomModifiers(int number)
        {
            bool notStackableUsed =false;
            List<Modifier> modifiersUsed = new List<Modifier>();
            for (int i = 0; i < number; i++)
            {
                Modifier newModifier = GetRandomModifier();

                while (modifiersUsed.Contains(newModifier) || modifiersUsed.Count>= modifierList.modifiers.Count || (!newModifier.stackable && notStackableUsed))
                {
                    newModifier = GetRandomModifier();
                }
                if (!newModifier.stackable)
                {
                    notStackableUsed = true;
                }
                modifiersUsed.Add(newModifier);
            }
            return modifiersUsed;
        }
        public int GetModifierIndex(Modifier modifier)
        {
            if (modifierList.modifiers.Contains(modifier))
            {
                return modifierList.modifiers.IndexOf(modifier);
            }
            else
            {
                Debug.Log("That modifier is not on the list");
                return 0;
            }
        }


        //llamadas alternativas
        public Modifier GetModifier(int modifierID)
        {
            return modifierList.GetModifier(modifierID);
        }
        public Sprite GetModifierIcon(int modifierID)
        {
            return GetModifier(modifierID).GetSprite();
        }
        public string GetModifierDescription(int modifierID)
        {
            return GetModifier(modifierID).GetDescription();
        }
        public string GetModifierTitle(int modifierID)
        {
            return GetModifier(modifierID).GetTitle();
        }
        public Sprite GetModifierIcon(Modifier modifier)
        {
            return modifier.GetSprite();
        }
        public string GetModifierDescription(Modifier modifier)
        {
            return modifier.GetDescription();
        }
        public string GetModifierTitle(Modifier modifier)
        {
            return modifier.GetTitle();
        }
    }
}