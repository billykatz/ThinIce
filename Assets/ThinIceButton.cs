using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ThinIceButton : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite normal;
    [SerializeField] private Sprite selected;

    public delegate void ClickAction();
    public event ClickAction OnClicked;


    public void ResetButton() {
        spriteRenderer.sprite = normal;
    }

    private void OnMouseDown() {
        spriteRenderer.sprite = selected;
    }

    private void OnMouseUp() {
        spriteRenderer.sprite = normal;
        if (OnClicked != null) {
            OnClicked();
        }
    }

    
}
