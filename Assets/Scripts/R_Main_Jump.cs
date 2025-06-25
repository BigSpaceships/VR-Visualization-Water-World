using System.Collections;
using System.Collections.Generic;
using Unity.Android.Gradle.Manifest;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class R_Main_Jump : MonoBehaviour {
    public InputActionProperty jumpButton;
    public float jumpForce = 5f;
    public GameObject xrOriginRig;
    public float groundCheckDistance = 1.8f;
    public float gravity = -9.81f;

    private CharacterController controller;
    private bool isGrounded = true;
    private float verticalSpeed = 0f;

    // Start is called before the first frame update
    void Start() {
        controller = xrOriginRig.GetComponent<CharacterController>();
        jumpButton.action.Enable();
        jumpButton.action.performed += ctx => jump();
    }

    // Update is called once per frame
    void OnDestroy() {
        jumpButton.action.performed -= ctx => jump();
    }
    void Update() {
        if (GamePublicV2.instance.moveMode != MoveMode.Ground) return;

        // 简单地向下发射射线判断是否着地
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance + 0.1f);

        if (isGrounded && verticalSpeed < 0)
            verticalSpeed = -0.5f;  // 微小值确保贴地

        verticalSpeed += gravity * Time.deltaTime;

        Vector3 move = new Vector3(0, verticalSpeed, 0);
        controller.Move(move * Time.deltaTime);
    }

    void jump() {
        if (GamePublicV2.instance.moveMode != MoveMode.Ground) return;
        if (isGrounded) {
            verticalSpeed = jumpForce;
        }
    }

    void OnCollisionEnter(Collision collision) {
        // 简单判断是否着地
        if (collision.contacts.Length > 0 && collision.contacts[0].normal.y > 0.5f) {
            isGrounded = true;
        }
    }
}
