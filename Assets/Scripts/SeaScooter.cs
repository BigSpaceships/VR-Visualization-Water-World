using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class SeaScooter : LocomotionProvider {
    [SerializeField] public InputActionReference activeScooderButton;


    [SerializeField] private Transform rightHandTransform;
    [SerializeField] private Transform cameraTransform;


    [SerializeField] private InputActionProperty rightHandMoveAction =
        new(new InputAction("Right Hand Move", expectedControlType: "Vector2"));


    private CharacterController _characterController;

    private float _timeAccelerating = 0;

    [SerializeField] private float minSpeed;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float accelerationRate;


    private float _moveSpeed;

    private void Start() {
        activeScooderButton.action.Enable();
    }

    private void OnEnable() {
        activeScooderButton?.action.Enable();
    }

    private void OnDisable() {
        activeScooderButton?.action.Disable();
    }



    protected override void Awake() {
        base.Awake();

        if (!_characterController) {
            var xrOrigin = system.xrOrigin?.Origin;
            if (xrOrigin != null) {
                xrOrigin.TryGetComponent(out _characterController);
            }
        }
    }

    private void Update() {
        // 只在水下模式才推进
        if (GamePublicV2.instance.moveMode != MoveMode.UnderWater) return;
        if (GamePublicV2.instance.cameraActive) return;

        // 直接读按钮的按下状态（值大于 0 就算按下）
        bool isPressed = activeScooderButton.action.ReadValue<float>() > 0.5f;
        if (isPressed) {
            // 累加加速时间，并计算当前速度
            _timeAccelerating += Time.deltaTime;
            _moveSpeed = Mathf.Clamp(accelerationRate * _timeAccelerating + minSpeed,
                                     minSpeed, maxSpeed);

            // 直接使用手柄的 forward（含上下方向），并归一化
            Vector3 direction = rightHandTransform.forward.normalized;

            // 最终运动向量
            Vector3 motion = direction * _moveSpeed * Time.deltaTime;

            if (CanBeginLocomotion() && BeginLocomotion()) {
                _characterController.Move(motion);
                EndLocomotion();
            }
        } else {
            // 松开按钮后重置加速时间
            _timeAccelerating = 0f;
        }
    }
}