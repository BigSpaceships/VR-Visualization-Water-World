using System.Collections;
using System.Collections.Generic;
using Unity.Android.Gradle;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

public class PC_MouseLook : MonoBehaviour {
    public Transform cameraTransform; // 绑定 Main Camera
    public float mouseSensitivity = 100f; // 鼠标灵敏度
    public float moveSpeedUnderWater = 2f; // 水底移动速度
    public float moveSpeedGround = 8f; // 陆地移动速度
    public float jumpForceGround = 5f;
    private float xRotation = 0f;
    private bool isVRActive = false;


    private bool isGrounded = false;

    private Vector3 moveDirection = Vector3.zero;
    private Rigidbody parentRb;
    void Awake() {
        parentRb = transform.parent.GetComponent<Rigidbody>();
    }

    void Start() {
        // 检测是否有 VR 设备
        isVRActive = XRSettings.isDeviceActive;

        if (!isVRActive) {
            Cursor.lockState = CursorLockMode.Locked; // 锁定鼠标
        }
    }

    private float mouseLockoutTimer = 0.1f;
    void Update() {
        isVRActive = XRSettings.isDeviceActive; // 动态检测是否接入 VR 设备

        if (!isVRActive) { // 仅当 VR 设备未连接时使用鼠标和键盘
            if (mouseLockoutTimer > 0f) {
                mouseLockoutTimer -= Time.deltaTime;
            } else {
                if (GamePublicV2.instance.moveMode == MoveMode.Ground) {
                    CheckGrounded();
                }
                HandleMouseLook();
                HandleKeyboardMovement();
            }
        }
    }

    void HandleMouseLook() {
        if (Mouse.current != null) {
            float mouseX = Mouse.current.delta.x.ReadValue() * mouseSensitivity * Time.deltaTime;
            float mouseY = Mouse.current.delta.y.ReadValue() * mouseSensitivity * Time.deltaTime;


            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            if (GamePublicV2.instance.moveMode == MoveMode.UnderWater) {
                transform.localRotation = Quaternion.Euler(xRotation, transform.localRotation.eulerAngles.y + mouseX, 0f);
            } else if (GamePublicV2.instance.moveMode == MoveMode.Ground) {
                cameraTransform.parent.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
                //cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
                transform.parent.Rotate(Vector3.up * mouseX);
            }
        }
    }

    void HandleKeyboardMovement() {
        moveDirection = Vector3.zero;

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        Vector3 flatForward = Vector3.ProjectOnPlane(forward, Vector3.up).normalized;
        Vector3 flatRight = Vector3.ProjectOnPlane(right, Vector3.up).normalized;

        // W/S
        if (Keyboard.current.wKey.isPressed)
            moveDirection += GamePublicV2.instance.moveMode == MoveMode.Ground ? flatForward : forward;
        if (Keyboard.current.sKey.isPressed)
            moveDirection -= GamePublicV2.instance.moveMode == MoveMode.Ground ? flatForward : forward;

        // A/D
        if (Keyboard.current.aKey.isPressed)
            moveDirection -= GamePublicV2.instance.moveMode == MoveMode.Ground ? flatRight : right;
        if (Keyboard.current.dKey.isPressed)
            moveDirection += GamePublicV2.instance.moveMode == MoveMode.Ground ? flatRight : right;

        // 上下（仅水下）
        if (GamePublicV2.instance.moveMode == MoveMode.UnderWater) {
            if (Keyboard.current.spaceKey.isPressed)
                moveDirection += Vector3.up;
            if (Keyboard.current.leftCtrlKey.isPressed)
                moveDirection -= Vector3.up;

            // ✅ 水下直接修改位置
            transform.parent.position += moveDirection.normalized * moveSpeedUnderWater * Time.deltaTime;
        }
        if (GamePublicV2.instance.moveMode == MoveMode.Ground
            && isGrounded
            && Keyboard.current.spaceKey.wasPressedThisFrame) {
            parentRb.AddForce(Vector3.up * jumpForceGround, ForceMode.Impulse);
        }
    }

    void FixedUpdate() {
        if (GamePublicV2.instance.moveMode != MoveMode.Ground) return;

        // 移动方向归零，避免摄像头 pitch 导致偏移
        moveDirection.y = 0;

        if (moveDirection != Vector3.zero) {
            Vector3 targetPos = parentRb.position + moveDirection.normalized * moveSpeedGround * Time.fixedDeltaTime;
            parentRb.MovePosition(targetPos);
        }
    }

    void CheckGrounded() {
        // 从刚体位置往下发射一条短射线检测是否接触地面
        isGrounded = Physics.Raycast(parentRb.position, Vector3.down, 1.01f);
    }
}
