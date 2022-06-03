using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ThinIceCanvasButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image image;
    [SerializeField] private Sprite normal;
    [SerializeField] private Sprite selected;
    [SerializeField] public string identifier;

    private bool isSelected = false;

    public static event Action<bool, string> OnClicked;


    public void ResetButton() {
        image.sprite = normal;
        isSelected = false;
    }

    public void SelectButton() {
        image.sprite = selected;
        isSelected = true;
    }

    public void OnPointerClick(PointerEventData eventData) {
        Debug.Log($"Custom Button clicked id {identifier} isSelected : {!isSelected}");
        isSelected = !isSelected;
        image.sprite = (isSelected) ? selected : normal;

        if (OnClicked != null) {
            OnClicked.Invoke(isSelected, identifier);
        }
    }
    
}
