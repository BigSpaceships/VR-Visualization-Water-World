using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour {
    public float mouseSensitivity = 100f;
    public Transform playerBody;

    private float xRotation = 0f;
    private InputAction lookAction;
    private InputAction rightClickAction;
    private bool isLooking = false; // 控制是否进行视角旋转的标志

    private void Awake() {
        // 获取 PlayerInput 组件并找到 "Look" Action
        var playerInput = GetComponent<PlayerInput>();
        lookAction = playerInput.actions["Look"];

        // 订阅事件
        lookAction.performed += OnLookPerformed;
        lookAction.canceled += OnLookCanceled;

        rightClickAction = playerInput.actions["RightClick"];
        rightClickAction.performed += _ => isLooking = true;
        rightClickAction.canceled += _ => isLooking = false;
    }

    private void OnDestroy() {
        // 取消订阅事件
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
        // 鼠标移动取消时的处理（如果有需要）
    }

    // 注释掉 Update 函数，因为我们现在通过事件进行处理
    // void Update() { ... }
}