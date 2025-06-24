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
        // ֻ��ˮ��ģʽ���ƽ�
        if (GamePublicV2.instance.moveMode != MoveMode.UnderWater) return;
        if (GamePublicV2.instance.cameraActive) return;

        // ֱ�Ӷ���ť�İ���״̬��ֵ���� 0 ���㰴�£�
        bool isPressed = activeScooderButton.action.ReadValue<float>() > 0.5f;
        if (isPressed) {
            // �ۼӼ���ʱ�䣬�����㵱ǰ�ٶ�
            _timeAccelerating += Time.deltaTime;
            _moveSpeed = Mathf.Clamp(accelerationRate * _timeAccelerating + minSpeed,
                                     minSpeed, maxSpeed);

            // ֱ��ʹ���ֱ��� forward�������·��򣩣�����һ��
            Vector3 direction = rightHandTransform.forward.normalized;

            // �����˶�����
            Vector3 motion = direction * _moveSpeed * Time.deltaTime;

            if (CanBeginLocomotion() && BeginLocomotion()) {
                _characterController.Move(motion);
                EndLocomotion();
            }
        } else {
            // �ɿ���ť�����ü���ʱ��
            _timeAccelerating = 0f;
        }
    }
}