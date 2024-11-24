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
    }
    public override void OnNetworkSpawn()
    {
        if(!IsServer)
        {
            RoundUI.Instance.PanelPowerUps = gameObject;
        }
        gameObject.SetActive(false);
    }
    private void OnEnable()
    {
        for (int i = 0; i < modifierSelectors.Count; i++)
        {
            modifierSelectors[i].ChooseModifier += SelectModifier;
        }
        if (IsServer)
        {
            GenerateNewModifiers();
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
    [ClientRpc]
    void SyncronizeModifiersClientRpc(int[] modificadores)
    {
        for (int i = 0; i < modifierSelectors.Count; i++)
        {
            modifierSelectors[i].ApplyModifier(ModifierRegistry.Instance.GetModifier(modificadores[i]));
            modifierSelectors[i].available = true;
        }
    }
    void GenerateNewModifiers()
    {
        modifiers = ModifierRegistry.Instance.GetRandomModifiers(6);
        List<int> indexModificadores = new List<int>();
        for (int i = 0; i < modifierSelectors.Count; i++)
        {
            indexModificadores.Add(ModifierRegistry.Instance.GetModifierIndex(modifiers[i]));
            modifierSelectors[i].ApplyModifier(modifiers[i]);
            modifierSelectors[i].available = true;
        }
        SyncronizeModifiersClientRpc(indexModificadores.ToArray());
    }
    public void ConfirmSelection()
    {
        RoundManager.Instance.EndPowerUpSelection();
    }
}
