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

    [SerializeField] private float sidewaysSpeed;
    [SerializeField] private float turnRateFromInput;
    [SerializeField] private float turnRateCorrective;

    [SerializeField] private float turnAngleStart;
    [Range(0,1)]
    [SerializeField] private float turnCorrectiveStart;

    private float _moveSpeed;

    [SerializeField] private float backSpeed;

    private void Start() {
        activeScooderButton.action.Enable();
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
        var projectedCameraForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up);
        var projectedControllerForward = Vector3.ProjectOnPlane(rightHandTransform.forward, Vector3.up);

        var lookRot = Quaternion.FromToRotation(projectedCameraForward, projectedControllerForward);

        var lookTurnAmount = Mathf.Min(lookRot.eulerAngles.y, 360 - lookRot.eulerAngles.y) *
                             Mathf.Sign(180 - lookRot.eulerAngles.y);

        var input = rightHandMoveAction.action.ReadValue<Vector2>();

        var lookTurnStrength = CalculateTurnSpeed(lookTurnAmount, input);

        var relativeMovement = GetMoveFromInput(input);

        var motion = rightHandTransform.right * relativeMovement.x + rightHandTransform.forward * relativeMovement.y;

        motion *= Time.deltaTime;

        var turnAmount = input.x * turnRateFromInput + lookTurnStrength * turnRateCorrective;
        
        turnAmount *= Time.deltaTime;

        if (relativeMovement != Vector2.zero) {
            if (CanBeginLocomotion() && BeginLocomotion()) {
                _characterController.Move(motion);

                system.xrOrigin.RotateAroundCameraUsingOriginUp(turnAmount);

                EndLocomotion();
            }
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private Vector2 GetMoveFromInput(Vector2 input) {
        //if (input == Vector2.zero)
        //    return Vector3.zero;

        input = rightHandMoveAction.action?.ReadValue<Vector2>() ?? Vector2.zero;

        if (GamePublic.cameraActive) {
            input.y = 0;
        } else {
            bool isPushingForward = activeScooderButton.action.IsPressed();
            if (isPushingForward) {
                input.y = 1;
            } else input.y = 0;
        }

        input.x *= sidewaysSpeed;

        if (input.y < 0) {
            _moveSpeed = backSpeed;

            _timeAccelerating = 0;
        }

        if (input.y == 0) {
            _timeAccelerating = 0;
        }

        if (input.y > 0) {
            _timeAccelerating += Time.deltaTime;

            _moveSpeed = CalculateSpeed();
        }
        input.y *= _moveSpeed;

        return input;
    }

    private float CalculateSpeed() {
        return Mathf.Clamp(accelerationRate * _timeAccelerating + minSpeed, minSpeed, maxSpeed);
    }
    
    private float CalculateTurnSpeed(float angleDist, Vector2 input) {
        var sign = Mathf.Sign(angleDist);
        
        var angle = Mathf.Abs(angleDist);
        
        angle -= turnAngleStart;

        angle = Mathf.Max(0, angle);

        var targetSpeed = sign * angle;

        // if there is forward input on the joystick, we want to turn. Otherwise, no turn
        var inputScale = Mathf.Max(0, input.y) >= turnCorrectiveStart ? 1 : 0;
        
        return targetSpeed * inputScale;
    }
}