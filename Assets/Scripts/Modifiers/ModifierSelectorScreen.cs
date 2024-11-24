using System.Collections;
using System.Collections.Generic;
using Tankito;
using Unity.Netcode;
using UnityEngine;

public class ModifierSelectorScreen : NetworkBehaviour
{
    [SerializeField]
    List<ModifierSelector> modifierSelectors;
    List<Modifier> modifiers;
    Modifier selectedModifier;
    void Start()
    {
            GenerateNewModifiers();
    }
    public override void OnNetworkSpawn()
    {
        if(!IsServer)
        {
            RoundManager.Instance._roundUI.PanelPowerUps = gameObject;
        }
        
    }
    private void OnEnable()
    {
        for (int i = 0; i < modifierSelectors.Count; i++)
        {
            modifierSelectors[i].ChooseModifier += SelectModifier;
        }
    }
    private void OnDisable()
    {
        for (int i = 0; i < modifierSelectors.Count; i++)
        {
            modifierSelectors[i].ChooseModifier -= SelectModifier;
        }
    }
    void SelectModifier(ModifierSelector modifierSelector)
    {

    }
    void GenerateNewModifiers()
    {
        modifiers = ModifierRegistry.Instance.GetRandomModifiers(6);
        for (int i = 0; i < modifierSelectors.Count; i++)
        {
            modifierSelectors[i].ApplyModifier(modifiers[i]);
            modifierSelectors[i].available = true;
        }
    }
    public void ConfirmSelection()
    {
        RoundManager.Instance.EndPowerUpSelection();
    }
}
