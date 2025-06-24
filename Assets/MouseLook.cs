using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour {
    public float mouseSensitivity = 100f;
    public Transform playerBody;

    private float xRotation = 0f;
    private InputAction lookAction;
    private InputAction rightClickAction;
    private bool isLooking = false; // �����Ƿ�����ӽ���ת�ı�־

    private void OnEnable() {
        // ��ȡ PlayerInput ������ҵ� "Look" Action
        var playerInput = GetComponent<PlayerInput>();
        lookAction = playerInput.actions["Look"];

        // �����¼�
        lookAction.performed += OnLookPerformed;
        lookAction.canceled += OnLookCanceled;

        rightClickAction = playerInput.actions["RightClick"];
        rightClickAction.performed += _ => isLooking = true;
        rightClickAction.canceled += _ => isLooking = false;
    }

    private void OnDisable()
    {
        // ȡ�������¼�
        lookAction.performed -= OnLookPerformed;
        lookAction.canceled -= OnLookCanceled;

        rightClickAction.performed -= _ => isLooking = true;
        rightClickAction.canceled -= _ => isLooking = false;
    }

    private void OnLookPerformed(InputAction.CallbackContext context) {
        if (isLooking) {
            Vector2 lookInput = context.ReadValue<Vector2>() * mouseSensitivity / 1000; //  Time.deltaTime;

            xRotation -= lookInput.y;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            //transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            //transform.Rotate(Vector3.right * lookInput.y);

            //playerBody.Rotate(Vector3.up * lookInput.x);
            playerBody.transform.Rotate(0, lookInput.x, 0, Space.World);
            playerBody.Rotate(-Vector3.right * lookInput.y);
            //Quaternion r = playerBody.rotation;
            //r.z = 0;
            //playerBody.rotation = r;
        }
    }

    private void OnLookCanceled(InputAction.CallbackContext context) {
        // ����ƶ�ȡ��ʱ�Ĵ������������Ҫ��
    }

    // ע�͵� Update ��������Ϊ��������ͨ���¼����д���
    // void Update() { ... }
}