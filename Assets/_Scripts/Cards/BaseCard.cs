using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public interface MouseInteractionDelegate {
    public void OnMouseDown();
    public void OnMouseEnter();
    public void OnMouseExit();
}

public class BaseCard : MonoBehaviour
{
    [SerializeField] public SpriteRenderer _spriteRenderer;

    [SerializeField] private InputActionReference DidSelect;
    [SerializeField] private InputActionReference MousePos;
    [SerializeField] public Collider collider;

    public ScriptableCard ScriptableCard;
    
    public string effectDescription;

    public MouseInteractionDelegate interactionDelegate;

    private bool _isHoveredOver;

    private void Start()
    {
        // The base card is the parent class of movement and modifier cards
        // Due to wonkiness with putting a box collider on both cards, I decided to only put the collider on one card
        // this means that we need to check to see if these are set before accessing them
        if (DidSelect != null)
        {
            DidSelect.action.performed += OnDidSelect;
        }

        if (MousePos != null)
        {
            MousePos.action.performed += MouseMoved;
        }
    }

    private void OnDestroy()
    {
        if (DidSelect != null)
        {
            DidSelect.action.performed -= OnDidSelect;
        }
        if (MousePos != null)
        {
            MousePos.action.performed -= MouseMoved;
        }
    }

    private void OnDidSelect(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Vector2 mousePos = MousePos.action.ReadValue<Vector2>();
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(mousePos.x, mousePos.y, 0.0f));
            RaycastHit hit;
            if (collider.Raycast(ray, out hit, Mathf.Infinity))
            {
                OnMouseDown();
            }
        }
    }
    
    private void MouseMoved(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Vector2 mousePos = MousePos.action.ReadValue<Vector2>();
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(mousePos.x, mousePos.y, 0.0f));
            RaycastHit hit;
            if (collider.Raycast(ray, out hit, Mathf.Infinity))
            {
                if (!_isHoveredOver)
                {
                    OnMouseEnter();
                }
                _isHoveredOver = true;
            }
            else
            {
                if (_isHoveredOver)
                {
                    OnMouseExit();
                }
                _isHoveredOver = false;
            }
        }
    }


    public void SetInteractionDelegate(MouseInteractionDelegate interactionDelegate) {
        this.interactionDelegate = interactionDelegate;
    }

    private void OnMouseDown() {
        if (interactionDelegate != null) {
           interactionDelegate.OnMouseDown(); 
        }
    }

    private void OnMouseExit() {
        if (interactionDelegate != null) {
           interactionDelegate.OnMouseExit(); 
        }

    }

     private void OnMouseEnter() {
        if (interactionDelegate != null) {
           interactionDelegate.OnMouseEnter(); 
        }

    }

    public virtual void PlayCard() {

    }
}
