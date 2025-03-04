using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

public class PC_MouseLook : MonoBehaviour {
    public Transform cameraTransform; // �� Main Camera
    public float mouseSensitivity = 100f; // ���������
    public float moveSpeed = 2f; // �ƶ��ٶ�
    private float xRotation = 0f;
    private bool isVRActive = false;

    void Start() {
        // ����Ƿ��� VR �豸
        isVRActive = XRSettings.isDeviceActive;

        if (!isVRActive) {
            Cursor.lockState = CursorLockMode.Locked; // �������
        }
    }

    void Update() {
        isVRActive = XRSettings.isDeviceActive; // ��̬����Ƿ���� VR �豸

        if (!isVRActive) // ���� VR �豸δ����ʱʹ�����ͼ���
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

        // �� W/S ���� "���������" �����ƶ���������ˮƽ�ƶ�
        if (Keyboard.current.wKey.isPressed)
            moveDirection += cameraTransform.forward; // ��������/�����ƶ�
        if (Keyboard.current.sKey.isPressed)
            moveDirection -= cameraTransform.forward;

        // �����ƶ���Ȼ��ˮƽ��
        if (Keyboard.current.aKey.isPressed)
            moveDirection -= cameraTransform.right;
        if (Keyboard.current.dKey.isPressed)
            moveDirection += cameraTransform.right;

        // �������½����ո��������� Ctrl �½���
        if (Keyboard.current.spaceKey.isPressed)
            moveDirection += Vector3.up;
        if (Keyboard.current.leftCtrlKey.isPressed)
            moveDirection -= Vector3.up;

        transform.position += moveDirection.normalized * moveSpeed * Time.deltaTime;
    }
}
