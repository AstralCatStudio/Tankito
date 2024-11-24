using System.Collections;
using System.Collections.Generic;
using Tankito;
using Unity.Netcode;
using UnityEngine;

public class ModifierSelectorScreen : MonoBehaviour
{
    [SerializeField]
    List<ModifierSelector> modifierSelectors;
    [SerializeField]
    RoundManager roundManager;
    List<Modifier> modifiers;
    Modifier selectedModifier;
    void Start()
    {
        transform.SetParent(GameObject.FindWithTag(tag: "Canvas").transform);
        GenerateNewModifiers();
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
        roundManager.EndPowerUpSelection();
    }
}
