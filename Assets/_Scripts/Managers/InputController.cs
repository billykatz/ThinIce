using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


[CreateAssetMenu(fileName = "New Scriptable InputController", menuName = "Input Controller")]
public class InputController : ScriptableObject
{
    
    [SerializeField] private InputActionReference DidSelect;
    [SerializeField] private InputActionReference MousePos;
    [SerializeField] private InputActionReference Drag;
    [SerializeField] private InputActionReference RightButton;
    [SerializeField] private InputActionReference MovementLeft;
    [SerializeField] private InputActionReference MovementRight;
    [SerializeField] private InputActionReference MovementUp;
    [SerializeField] private InputActionReference MovementDown;

    private void Awake()
    {
        DidSelect.action.Enable();
        MousePos.action.Enable();
        Drag.action.Enable();
        RightButton.action.Enable();
        MovementLeft.action.Enable();
        MovementRight.action.Enable();
        MovementUp.action.Enable();
        MovementDown.action.Enable();
    }
}
