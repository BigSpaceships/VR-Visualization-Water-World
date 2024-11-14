using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class SeaScooter : LocomotionProvider {
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

    private float _moveSpeed;

    [SerializeField] private float backSpeed;

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
        var relativeMovement = GetMoveFromInput(rightHandMoveAction.action.ReadValue<Vector2>()) * Time.deltaTime;

        var motion = rightHandTransform.right * relativeMovement.x + rightHandTransform.forward * relativeMovement.y;

        var projectedCameraForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up);
        var projectedControllerForward = Vector3.ProjectOnPlane(rightHandTransform.forward, Vector3.up);

        var lookRot = Quaternion.FromToRotation(projectedCameraForward, projectedControllerForward);

        var lookTurnAmount = Mathf.Min(lookRot.eulerAngles.y, 360 - lookRot.eulerAngles.y) *
                             Mathf.Sign(180 - lookRot.eulerAngles.y) * Time.deltaTime;
        // TODO: use an easing function for this because it can get a bit jarring

        var turnAmount = relativeMovement.x * turnRateFromInput + lookTurnAmount * turnRateCorrective;

        if (relativeMovement != Vector2.zero) {
            if (CanBeginLocomotion() && BeginLocomotion()) {
                _characterController.Move(motion);

                system.xrOrigin.RotateAroundCameraUsingOriginUp(turnAmount);

                EndLocomotion();
            }
        }
    }

    private Vector2 GetMoveFromInput(Vector2 input) {
        if (input == Vector2.zero)
            return Vector3.zero;

        input = rightHandMoveAction.action?.ReadValue<Vector2>() ?? Vector2.zero;

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
        return maxSpeed - (maxSpeed - minSpeed) * Mathf.Exp(-accelerationRate * _timeAccelerating);
    }
}