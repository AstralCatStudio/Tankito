using System.Collections;
using System.Collections.Generic;
using Tankito;
using Tankito.Netcode;
using UnityEngine;

public class ModifiersController : MonoBehaviour
{
    public List<Modifier> modifiers;
    public BulletCannon bulletCreator;
    public TankController tankController;
    private void Start()
    {
        applyModifiers();
    }

    void applyModifiers()
    {
        foreach (Modifier modifier in modifiers)
        {
            foreach (BulletModifier bulletModifier in modifier.bulletModifiers)
            {
                bulletCreator.m_bulletModifiers.Add(bulletModifier);
            }
        }
        bulletCreator.ApplyModifierProperties();
        //TODO: aplicar tambi�n los del tanque
    }

}
