using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class KeyboardMovement : MonoBehaviour {
    private PlayerInput playerInput;
    public float moveSpeed = 5f; // ����ƶ��ٶ�
    private bool moveForward = false; // �����ƶ��Ĳ���ֵ
    private bool moveBackward = false;
    private bool moveLeft = false;
    private bool moveRight = false;

    private void OnEnable() {
        playerInput = GetComponent<PlayerInput>();

        // ��ȡMoveForward Action������
        InputAction moveForwardAction = playerInput.actions["MoveForward"];
        InputAction moveBackwardAction = playerInput.actions["MoveBackward"];
        InputAction moveLeftAction = playerInput.actions["MoveLeft"];
        InputAction moveRightAction = playerInput.actions["MoveRight"];

        // ע���¼�
        moveForwardAction.performed += OnMoveForward;
        moveForwardAction.canceled += OnMoveForward;
        moveBackwardAction.performed += OnMoveBackward;
        moveBackwardAction.canceled += OnMoveBackward;
        moveLeftAction.performed += OnMoveLeft;
        moveLeftAction.canceled += OnMoveLeft;
        moveRightAction.performed += OnMoveRight;
        moveRightAction.canceled += OnMoveRight;
    }

    private void OnDisable() {
        // ��ȡMoveForward Action������
        InputAction moveForwardAction = playerInput.actions["MoveForward"];
        InputAction moveBackwardAction = playerInput.actions["MoveBackward"];
        InputAction moveLeftAction = playerInput.actions["MoveLeft"];
        InputAction moveRightAction = playerInput.actions["MoveRight"];

        // ע���¼�
        moveForwardAction.performed -= OnMoveForward;
        moveForwardAction.canceled -= OnMoveForward;
        moveBackwardAction.performed -= OnMoveBackward;
        moveBackwardAction.canceled -= OnMoveBackward;
        moveLeftAction.performed -= OnMoveLeft;
        moveLeftAction.canceled -= OnMoveLeft;
        moveRightAction.performed -= OnMoveRight;
        moveRightAction.canceled -= OnMoveRight;
    }

    private void Update()
    {
        Vector3 moveDirection = Vector3.zero;

        // ���ݲ���ֵȷ���ƶ�����
        if (moveForward)
        {
            moveDirection += transform.forward;
        }
        if (moveBackward)
        {
            moveDirection -= transform.forward;
        }
        if (moveLeft)
        {
            moveDirection -= transform.right;
        }
        if (moveRight)
        {
            moveDirection += transform.right;
        }

        // Ӧ���ƶ�
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