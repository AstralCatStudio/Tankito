using System.Collections;
using System.Collections.Generic;
using Tankito;
using Tankito.Netcode;
using UnityEngine;

public class ModifiersController : MonoBehaviour
{
    public List<Modifier> modifiers;
    public BulletCannon m_bulletCannon;
    public TankController tankController;
    private void Start()
    {
        ApplyModifiers();
    }
    public void AddModifier(Modifier modifier)
    {
        modifiers.Add(modifier);
        ApplyModifiers();
    }
    void ApplyModifiers()
    {
        foreach (Modifier modifier in modifiers)
        {
            foreach (BulletModifier bulletModifier in modifier.bulletModifiers)
            {
                m_bulletCannon.Modifiers.Add(bulletModifier);
            }

            foreach(HullModifier hullMod in modifier.hullModifiers)
            {
                
            }
        }
        m_bulletCannon.ApplyModifierProperties();
        //TODO: aplicar tambi�n los del tanque
    }

}
