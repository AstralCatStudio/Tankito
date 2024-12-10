using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;
namespace Tankito
{
    public class ModifiersUserProfile : NetworkBehaviour
    {
        public ulong ownerId;
        TankData tankConnected;
        ModifiersController modifiersController;
        [SerializeField]
        TextMeshProUGUI userName, pointsText;
        [SerializeField]
        Image characterImage;
        [SerializeField]
        List<Image> modifierImages;
        [SerializeField]
        Transform modifierImagesParent;
        public void SetValues()
        {
            tankConnected = RoundManager.Instance.Players[OwnerClientId];
            modifiersController = tankConnected.GetComponent<ModifiersController>();
            characterImage.sprite = ClientData.Instance.characters[tankConnected.SkinSelected].data.sprite;
            Debug.Log(tankConnected.Username);
            userName.text = tankConnected.Username;
            pointsText.text = "Points: " + tankConnected.Points.ToString();
            SetModifiers(modifiersController.modifiers);
            if (IsServer)
            {
                SetValuesClientRpc(ownerId);
            }
        }
        [ClientRpc]
        void SetValuesClientRpc(ulong owner)
        {
            if (!IsServer)
            {
                ownerId = owner;
                SetValues();
            }
        }
        public void SetUserName(string name)
        {
            userName.text = name;
        }
        public void SetPoints(int points)
        {
            pointsText.text = "Points: " + points;
        }
        public void SetCharacterImage(Sprite sprite)
        {
            characterImage.sprite = sprite;
        }
        public void SetModifiers(List<Modifier> modifiers)
        {
            if (modifiers.Count != modifierImages.Count)
            {
                modifierImages.Clear();
            }
            for (int i = 0; i < modifiers.Count; i++)
            {
                
                if (modifierImagesParent.GetChild(i)!=null)
                {
                    modifierImagesParent.GetChild(i).gameObject.SetActive(true);
                    modifierImages.Add(modifierImagesParent.GetChild(i).gameObject.GetComponent<Image>());

                    modifierImages[i] = modifierImagesParent.GetChild(i).gameObject.GetComponent<Image>();
                }
                else
                {
                    GameObject modifierImage = Instantiate<GameObject>(new GameObject(), modifierImagesParent);
                    modifierImage.AddComponent<Image>();
                    modifierImages.Add(modifierImagesParent.GetChild(i).gameObject.GetComponent<Image>());
                    modifierImages[i] = modifierImage.GetComponent<Image>(); 
                }
                modifierImages[i].sprite = modifiers[i].GetSprite();
            }
            for (int i = modifiers.Count; i < modifierImagesParent.childCount; i++)
            {
                modifierImagesParent.GetChild(i).gameObject.SetActive(false);
            }
        }
    }
}
