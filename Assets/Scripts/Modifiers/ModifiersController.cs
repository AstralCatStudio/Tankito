using System.Collections;
using System.Collections.Generic;
using Tankito;
using Tankito.Netcode;
using UnityEngine;

public class ModifiersController : MonoBehaviour
{
    public List<Modifier> modifiers;
    public CreateBullet bulletCreator;
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
                bulletCreator.modifiers.Add(bulletModifier);
            }
        }
        bulletCreator.applyModifierProperties();
        //TODO: aplicar también los del tanque
    }

}
