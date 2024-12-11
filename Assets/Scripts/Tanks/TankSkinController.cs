using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TankSkinController : NetworkBehaviour
{
    [SerializeField]
    SpriteRenderer hull, cannon, fish;
    [SerializeField] Animator fishAnimator;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner)
        {
            SetOwnedSkin();
        }
    }
    public void SetOwnedSkin()
    {
        if (IsOwner)
        {
            SetSkin(ClientData.Instance.characters.IndexOf(ClientData.Instance.GetCharacterSelected()));
        }
    }
    public int GetOwnedSkin()
    {
        return ClientData.Instance.characters.IndexOf(ClientData.Instance.GetCharacterSelected());
    }
    void SetSkin(int index)
    {
        CharacterData data = ClientData.Instance.characters[index].data;
        hull.sprite = data.tankBody;
        cannon.sprite = data.tankHead;
        //fish.sprite = data.fishSprite;
        fishAnimator.runtimeAnimatorController = data.fishAnimator;

        if (IsOwner)
        {
            SetSkinServerRpc(index);
        }
    }
    [ServerRpc]
    void SetSkinServerRpc(int index)
    {
        if (!IsOwner)
        {
            SetSkin(index);
        }
        SetSkinClientRpc(index);
    }
    [ClientRpc]
    void SetSkinClientRpc(int index)
    {
        if (!IsOwner && !IsServer)
        {
            SetSkin(index);
        }
    }
}
