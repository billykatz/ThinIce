using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class ThinIceCanvasButton : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private InputActionReference _pointerClicked;
    [SerializeField] private InputActionReference _pointerPos;
    [SerializeField] public string identifier;
    [SerializeField] public UnityEvent WasClicked;
    [SerializeField] public EventSystem _eventSystem;
    [SerializeField] public GraphicRaycaster _raycaster;


    private void Start()
    {
        _pointerClicked.action.performed += PointerDidClick;
    }

    private void PointerDidClick(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            var pos = _pointerPos.action.ReadValue<Vector2>();
            var pointerEventData = new PointerEventData(_eventSystem) { position = pos };
 
            var results = new List<RaycastResult>();
            _raycaster.Raycast(pointerEventData, results);

            for (int i = 0; i < results.Count; i++)
            {
                if (results[i].gameObject == gameObject)
                {
                    Debug.Log("ive been clicked");
                    WasClicked.Invoke();
                }
            }
        }
    }
}
