using System.Collections.Generic;
using UnityEngine;

namespace Tankito
{
    public enum ModifierName
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
    }
}