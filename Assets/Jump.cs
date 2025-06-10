using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Jump : MonoBehaviour {

    [SerializeField] private InputActionProperty jumpButton;
    [SerializeField] private float jumpHeight = 3f;
    [SerializeField] private CharacterController cc;
    [SerializeField] private LayerMask groundLayers;

    private float gravity = Physics.gravity.y;
    private Vector3 movement;

    private void Update() {
        bool _isGrounded = IsGrounded();

        if (jumpButton.action.WasPressedThisFrame() && _isGrounded) {
            ToJump();
        }

        movement.y += gravity * Time.deltaTime;

        cc.Move(movement * Time.deltaTime);
    }

    public void JumpRequest() {
        if (IsGrounded()) {
            ToJump();
        }
    }

    public void ToJump() {
        movement.y = Mathf.Sqrt(jumpHeight * -3.0f * gravity);
    }


    private bool IsGrounded() {
        return Physics.CheckSphere(transform.position, 0.2f, groundLayers);
    }

}