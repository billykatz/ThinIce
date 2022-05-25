using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public interface MouseInteractionDelegate {
    public void OnMouseDown();
    public void OnMouseEnter();
    public void OnMouseExit();
}

public class BaseCard : MonoBehaviour
{
    [SerializeField] protected SpriteRenderer _spriteRenderer;

    public MouseInteractionDelegate interactionDelegate;

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
