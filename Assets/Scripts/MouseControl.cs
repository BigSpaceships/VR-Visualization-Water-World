using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseControl : MonoBehaviour {
    [SerializeField] private float speed;
    [SerializeField] private float mouseSpeed;
    
    public InputActionAsset actions;
    
    public InputActionProperty moveAction = new InputActionProperty(new InputAction("Move"));
    public InputActionProperty lookAction = new InputActionProperty(new InputAction("Look"));

    private void OnEnable() {
        actions.Enable();
        
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update() {
        var input = moveAction.action.ReadValue<Vector2>();
        
        transform.Translate(new Vector3(input.x, 0, input.y) * (speed * Time.deltaTime));
        
        var mouse = lookAction.action.ReadValue<Vector2>();

        transform.Rotate(Vector3.up, mouse.x * mouseSpeed, Space.World);
        transform.Rotate(Vector3.right, -mouse.y * mouseSpeed, Space.Self);
    }
}
