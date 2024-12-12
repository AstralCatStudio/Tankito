using System;
using System.Collections;
using System.Collections.Generic;
using Tankito;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ShellAnimation : MonoBehaviour
{
    [SerializeField] private Sprite shellOpened;
    [SerializeField] private Sprite shellClosed;
    [SerializeField] private int numOfTurns = 2;
    [SerializeField] private float degrees = 30;
    [SerializeField] private float waitTime = 0.5f;
    [SerializeField] private float animationTime = 0.5f;

    public bool selected;
    public bool alreadyTaken;
    public Modifier modifier;
    public AnimatedModifiers potenciadoresGameObject;

    public GameObject description;
    public GameObject modifierObject;

    public Action onAnimationFinished;
    public GameObject notStackable;

    private void Awake()
    {
        potenciadoresGameObject = FindObjectOfType<AnimatedModifiers>();
        if (potenciadoresGameObject == null)
        {
            Debug.LogWarning("Objeto no encontrado");
        }
    }

    private void OnEnable()
    {
        
        Invoke("StartAnimation", waitTime);
    }

    public void StartAnimation()
    {
        StartCoroutine(AnimateShell());
    }

    private void OnDisable()
    {
        gameObject.GetComponent<Image>().sprite = shellClosed;
        description.SetActive(false);
        modifierObject.SetActive(false);
    }

    private IEnumerator AnimateShell()
    {
        RectTransform rt = GetComponent<RectTransform>();
        float turn = 0;
        LeanTween.rotateAroundLocal(rt, Vector3.forward, degrees / 2, animationTime / (numOfTurns * 2));
        yield return new WaitForSeconds(animationTime / (numOfTurns * 2));
        while (turn < numOfTurns)
        {
            Debug.Log(turn);
            if (turn % 2 == 0)
            {
                LeanTween.rotateAroundLocal(rt, Vector3.forward, -degrees, animationTime / (numOfTurns * 2));
            } else
            {
                LeanTween.rotateAroundLocal(rt, Vector3.forward, degrees, animationTime / (numOfTurns * 2));
            }
            turn++;
            yield return new WaitForSeconds(animationTime / (numOfTurns * 2));
        }
        LeanTween.rotateAroundLocal(rt, Vector3.forward, -degrees / 2, animationTime / (numOfTurns * 2));
        Invoke("AnimationFinished", animationTime / 2);
    }

    private void AnimationFinished()
    {
        onAnimationFinished?.Invoke();
        Enable();
    }

    public void Enable()
    {
        if(!alreadyTaken)
        {
            GetComponent<Image>().sprite = shellOpened;
            GetComponent<Button>().enabled = true;
            GetComponent<HoverButton>().enabled = true;
            if (!modifier.stackable)
            {
                notStackable.SetActive(true);
            }
            else
            {
                notStackable.SetActive(false);
            }
            description.SetActive(true);
            modifierObject.SetActive(true);
            modifierObject.transform.GetChild(0).GetComponent<Image>().sprite = modifier.GetSpriteNoBackground();  //obtiene el sprite del modificador y lo aplica en el icono del shell
            description.GetComponent<TextMeshProUGUI>().text = modifier.GetDescription();   //obtiene la descripcion
        }
    }

    public void EnableButton()
    {
        if (!alreadyTaken)
        {
            GetComponent<Button>().enabled = true;
            GetComponent<HoverButton>().enabled = true;
        }
    }

    public void Select()
    {
        if (!alreadyTaken)
        {
            MusicManager.Instance.PlaySound("aceptar2");

            potenciadoresGameObject.DeselectAllModifiers();
            gameObject.GetComponent<Outline>().enabled = true;
            selected = true;
        }
        else
        {
            Debug.LogWarning("This modifier is already taken.");
        }
    }

    public void DisableButton()
    {
        GetComponent<Button>().enabled = false;
        GetComponent<HoverButton>().enabled = false;
        gameObject.GetComponent<Outline>().enabled = false;
    }

    public void Disable()
    {
        GetComponent<Image>().sprite = shellClosed;
        GetComponent<Button>().enabled = false;
        GetComponent<HoverButton>().enabled = false;
        description.SetActive(false);
        modifierObject.SetActive(false);
    }

    public void SetAlreadyTaken(bool taken) { alreadyTaken = taken; }
}
