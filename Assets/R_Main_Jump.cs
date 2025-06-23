using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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
    }

    void Update() {
        // �򵥵����·��������ж��Ƿ��ŵ�
        if (SceneManager.GetSceneByName("R_Area2 Under Water").isLoaded) return;
        if (jumpButton.action.WasPressedThisFrame()) jump();
        isGrounded = Physics.Raycast(xrOriginRig.transform.position, -xrOriginRig.transform.up, groundCheckDistance + 0.1f);

        if (isGrounded && verticalSpeed < 0)
            verticalSpeed = 0;  // ΢Сֵȷ������

        verticalSpeed += gravity * Time.deltaTime;

        Vector3 move = new Vector3(0, verticalSpeed, 0);
        controller.Move(move * Time.deltaTime);
    }

    void jump() {
        if (isGrounded) {
            verticalSpeed = jumpForce;
        }
    }

    void OnCollisionEnter(Collision collision) {
        // ���ж��Ƿ��ŵ�
        if (collision.contacts.Length > 0 && collision.contacts[0].normal.y > 0.5f) {
            isGrounded = true;
        }
    }
}
