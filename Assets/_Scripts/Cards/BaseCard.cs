using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public interface MouseInteractionDelegate {
    public void OnMouseDown();
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
}
