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

    private bool isSelected = false;

    public delegate void ClickAction();
    public event ClickAction OnClicked;


    public void ResetButton() {
        image.sprite = normal;
    }

    public void OnPointerClick(PointerEventData eventData) {
        Debug.Log("Custom Button clicked");
        isSelected = !isSelected;
        image.sprite = (isSelected) ? selected : normal;
    }
    
}
