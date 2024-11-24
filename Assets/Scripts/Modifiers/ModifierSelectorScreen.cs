using System.Collections;
using System.Collections.Generic;
using Tankito;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ModifierSelectorScreen : NetworkBehaviour
{
    [SerializeField]
    List<ModifierSelector> modifierSelectors;
    List<Modifier> modifiers;
    ModifierSelector selectedModifier;
    [SerializeField]
    TextMeshProUGUI playerChoosePowerupText;
    int tankorder = 0;
    List<ulong> tanksInOrder = new List<ulong>(4);
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
            tankorder = 0;
            GenerateNewModifiers();
            List<TankData>  tankDatasInOrder = RoundManager.Instance.GetTankOrder();
            tanksInOrder.Clear();
            for (int i = 0; i < tankDatasInOrder.Count; i++)
            {
                tanksInOrder.Add(tankDatasInOrder[i].OwnerClientId);
                tanksInOrder[i] = tankDatasInOrder[i].OwnerClientId;
            }
            SetSelectorPlayer(tankorder);
        }
    }
    void SetSelectorPlayer(int playerOrder)
    {
        playerChoosePowerupText.text = "Player "+ tanksInOrder[playerOrder] + " Choose a Power Up";
        SetSelectorPlayerClientRpc(tanksInOrder[playerOrder]);
    }

    [ClientRpc]
    void SetSelectorPlayerClientRpc(ulong playerClientId)
    {
        playerChoosePowerupText.text = "Player " + playerClientId + " Choose a Power Up";
        if (NetworkManager.LocalClientId != playerClientId)
        {
            foreach (ModifierSelector modifierSelector in modifierSelectors)
            {

                modifierSelector.GetComponent<Button>().interactable = false;
            }
        }
        else
        {
            foreach (ModifierSelector modifierSelector in modifierSelectors)
            {
                if (modifierSelector.available)
                {
                    modifierSelector.GetComponent<Button>().interactable = true;
                }
                else
                {
                    modifierSelector.GetComponent<Button>().interactable = false;
                }
                
            }
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
        selectedModifier = modifierSelector;
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
        ConfirmSelectionServerRpc(NetworkManager.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ConfirmSelectionServerRpc(ulong playerClientId)
    {
        if(tankorder == tanksInOrder.IndexOf(playerClientId) && selectedModifier.available)
        {
            selectedModifier.available = false;
            AddModifierClientRpc(playerClientId, modifierSelectors.IndexOf(selectedModifier));
            tankorder++;
            if (tankorder >= tanksInOrder.Count)
            {
                RoundManager.Instance.EndPowerUpSelection();
            }
            else
            {
                SetSelectorPlayer(tankorder);
            }
        }
    }

    [ClientRpc]
    void AddModifierClientRpc(ulong playerClientId, int modifierSelectorId)
    {
        modifierSelectors[modifierSelectorId].available = false;
        BulletCannonRegistry.Instance[playerClientId].transform.parent.parent.parent.GetComponent<ModifiersController>().AddModifier(modifierSelectors[modifierSelectorId].GetModifier());
    }
}
