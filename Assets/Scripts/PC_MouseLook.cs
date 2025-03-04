using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

public class PC_MouseLook : MonoBehaviour {
    public Transform cameraTransform; // 绑定 Main Camera
    public float mouseSensitivity = 100f; // 鼠标灵敏度
    public float moveSpeed = 2f; // 移动速度
    private float xRotation = 0f;
    private bool isVRActive = false;

    void Start() {
        // 检测是否有 VR 设备
        isVRActive = XRSettings.isDeviceActive;

        if (!isVRActive) {
            Cursor.lockState = CursorLockMode.Locked; // 锁定鼠标
        }
    }

    void Update() {
        isVRActive = XRSettings.isDeviceActive; // 动态检测是否接入 VR 设备

        if (!isVRActive) // 仅当 VR 设备未连接时使用鼠标和键盘
        {
            HandleMouseLook();
            HandleKeyboardMovement();
        }
    }

    void HandleMouseLook() {
        if (Mouse.current != null) {
            float mouseX = Mouse.current.delta.x.ReadValue() * mouseSensitivity * Time.deltaTime;
            float mouseY = Mouse.current.delta.y.ReadValue() * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            cameraTransform.localRotation = Quaternion.Euler(xRotation, cameraTransform.localRotation.eulerAngles.y + mouseX, 0f);
        }
    }

    void HandleKeyboardMovement() {
        Vector3 moveDirection = Vector3.zero;

        // 让 W/S 按照 "摄像机朝向" 方向移动，而不是水平移动
        if (Keyboard.current.wKey.isPressed)
            moveDirection += cameraTransform.forward; // 允许向上/向下移动
        if (Keyboard.current.sKey.isPressed)
            moveDirection -= cameraTransform.forward;

        // 左右移动仍然是水平的
        if (Keyboard.current.aKey.isPressed)
            moveDirection -= cameraTransform.right;
        if (Keyboard.current.dKey.isPressed)
            moveDirection += cameraTransform.right;

        // 上升和下降（空格上升，左 Ctrl 下降）
        if (Keyboard.current.spaceKey.isPressed)
            moveDirection += Vector3.up;
        if (Keyboard.current.leftCtrlKey.isPressed)
            moveDirection -= Vector3.up;

        transform.position += moveDirection.normalized * moveSpeed * Time.deltaTime;
    }
}
