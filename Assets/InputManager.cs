using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{

    [SerializeField] private InputActionReference DidSelect;
    [SerializeField] private InputActionReference MousePos;

    private void Awake()
    {
        DidSelect.action.Enable();
        MousePos.action.Enable();
    }
}
