using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class KeyboardMovement : MonoBehaviour {
    private PlayerInput playerInput;
    public float moveSpeed = 5f; // 玩家移动速度
    private bool moveForward = false; // 控制移动的布尔值
    private bool moveBackward = false;
    private bool moveLeft = false;
    private bool moveRight = false;

    private void Awake() {
        playerInput = GetComponent<PlayerInput>();

        // 获取MoveForward Action的引用
        InputAction moveForwardAction = playerInput.actions["MoveForward"];
        InputAction moveBackwardAction = playerInput.actions["MoveBackward"];
        InputAction moveLeftAction = playerInput.actions["MoveLeft"];
        InputAction moveRightAction = playerInput.actions["MoveRight"];

        // 注册事件
        moveForwardAction.performed += OnMoveForward;
        moveForwardAction.canceled += OnMoveForward;
        moveBackwardAction.performed += OnMoveBackward;
        moveBackwardAction.canceled += OnMoveBackward;
        moveLeftAction.performed += OnMoveLeft;
        moveLeftAction.canceled += OnMoveLeft;
        moveRightAction.performed += OnMoveRight;
        moveRightAction.canceled += OnMoveRight;
    }

    private void OnDestroy() {
        // 获取MoveForward Action的引用
        InputAction moveForwardAction = playerInput.actions["MoveForward"];
        InputAction moveBackwardAction = playerInput.actions["MoveBackward"];
        InputAction moveLeftAction = playerInput.actions["MoveLeft"];
        InputAction moveRightAction = playerInput.actions["MoveRight"];

        // 注销事件
        moveForwardAction.performed -= OnMoveForward;
        moveForwardAction.canceled -= OnMoveForward;
        moveBackwardAction.performed -= OnMoveBackward;
        moveBackwardAction.canceled -= OnMoveBackward;
        moveLeftAction.performed -= OnMoveLeft;
        moveLeftAction.canceled -= OnMoveLeft;
        moveRightAction.performed -= OnMoveRight;
        moveRightAction.canceled -= OnMoveRight;
    }

    private void Update() {
        Vector3 moveDirection = Vector3.zero;

        // 根据布尔值确定移动方向
        if (moveForward) {
            moveDirection += transform.forward;
        }
        if (moveBackward) {
            moveDirection -= transform.forward;
        }
        if (moveLeft) {
            moveDirection -= transform.right;
        }
        if (moveRight) {
            moveDirection += transform.right;
        }

        // 应用移动
        transform.position += moveDirection.normalized * moveSpeed * Time.deltaTime;
    }

    public void OnMoveForward(InputAction.CallbackContext context) {
        moveForward = context.performed;
    }

    public void OnMoveBackward(InputAction.CallbackContext context) {
        moveBackward = context.performed;
    }

    public void OnMoveLeft(InputAction.CallbackContext context) {
        moveLeft = context.performed;
    }

    public void OnMoveRight(InputAction.CallbackContext context) {
        moveRight = context.performed;
    }
}