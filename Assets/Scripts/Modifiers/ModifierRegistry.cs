using System.Collections.Generic;
using UnityEngine;

namespace Tankito
{
    public enum ModifierName //IMPORTANTE: si se añaden modificadores añadirlos a este enum con el index en la lista de modificadores
    {
        Test=0,
        Shotgun=1,
        NoFriendlyFire=2,
        FasterTank=3,
        FasterBullets=4,
        MoreBullets=5,
        LongerParry=6,
        LongerDash=7,
        LessReloadTime=8,
        LowerParryCooldown=9,
        LowerDashCooldown=10,
        FasterDash=11,
        OppositeBullet=12,
        ExplosiveBullets=13,
        BouncyBullets=14,
        BigBullets=15
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
        public Modifier GetRandomModifier()
        {
            int randomModifier = Random.Range(0, modifierList.modifiers.Count);
            return GetModifier(randomModifier);
        }
        public List<Modifier> GetRandomModifiers(int number)
        {
            List<Modifier> modifiersUsed = new List<Modifier>();
            for (int i = 0; i < number; i++)
            {

            }
            return modifiersUsed;
        }



        //llamadas alternativas con int por si acaso
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
    }
}