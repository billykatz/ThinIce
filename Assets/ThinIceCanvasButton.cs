using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class ThinIceCanvasButton : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Sprite normal;
    [SerializeField] private Sprite selected;
    [SerializeField] private InputAction DidSelect;
    [SerializeField] public string identifier;
    
    private bool isSelected;
    
    public static event Action<bool, string> OnClicked;

    private void Start()
    {
        DidSelect.Enable();
        DidSelect.performed += ctx => OnClick();
    }


    public void ResetButton() {
        image.sprite = normal;
        isSelected = false;
    }

    public void SelectButton() {
        image.sprite = selected;
        isSelected = true;
    }

    public void OnClick() {
        Debug.Log($"Custom Button clicked id {identifier} isSelected : {!isSelected}");
        isSelected = !isSelected;
        image.sprite = (isSelected) ? selected : normal;

        if (OnClicked != null) {
            OnClicked.Invoke(isSelected, identifier);
        }
    }
    
}
