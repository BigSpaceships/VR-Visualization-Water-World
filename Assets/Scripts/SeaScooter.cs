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

    [SerializeField] private float turnAngleStart;
    [SerializeField] private float turnAngleEnd;
    [SerializeField] private float turnAngleStartStrength;

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
        var projectedCameraForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up);
        var projectedControllerForward = Vector3.ProjectOnPlane(rightHandTransform.forward, Vector3.up);

        var lookRot = Quaternion.FromToRotation(projectedCameraForward, projectedControllerForward);

        var lookTurnAmount = Mathf.Min(lookRot.eulerAngles.y, 360 - lookRot.eulerAngles.y) *
                             Mathf.Sign(180 - lookRot.eulerAngles.y);

        var lookTurnStrength = CalculateTurnSpeed(lookTurnAmount);

        var relativeMovement = GetMoveFromInput(rightHandMoveAction.action.ReadValue<Vector2>(), lookTurnStrength);

        Debug.Log(relativeMovement);

        var motion = rightHandTransform.right * relativeMovement.x + rightHandTransform.forward * relativeMovement.y;

        var turnAmount = relativeMovement.x * turnRateFromInput + lookTurnStrength * turnRateCorrective * Time.deltaTime;

        if (relativeMovement != Vector2.zero) {
            if (CanBeginLocomotion() && BeginLocomotion()) {
                _characterController.Move(motion);

                system.xrOrigin.RotateAroundCameraUsingOriginUp(turnAmount);

                EndLocomotion();
            }
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private Vector2 GetMoveFromInput(Vector2 input, float sidewaysFactor) {
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

        // Debug.Log(1.5f - sidewaysFactor);

        input.y *= _moveSpeed * (1.5f - Mathf.Abs(sidewaysFactor));

        return input;
    }

    private float CalculateSpeed() {
        return maxSpeed - (maxSpeed - minSpeed) * Mathf.Exp(-accelerationRate * _timeAccelerating);
    }

    private float CalculateBlend(float t) {
        float result;
        if (t < 0.5) {
            result = 2 * t * t;
        }
        else {
            result = 2 * (t - .5f) * (1.5f - t) + 0.5f;
        }

        return Mathf.Clamp01(result);
    }

    private float CalculateTurnSpeed(float angleDist) {
        var sign = Mathf.Sign(angleDist);
        angleDist = Mathf.Abs(angleDist);

        var func1 = Mathf.Pow((angleDist - turnAngleStart) / turnAngleStartStrength, 3);

        if (angleDist < turnAngleStart) {
            func1 = 0;
        }

        var func2 = Mathf.Pow(angleDist / turnAngleEnd, 1 / 3f);

        var transformedTForBlend = Mathf.Clamp01((angleDist - turnAngleStart) / (turnAngleEnd - turnAngleStart));

        var blendFactor = CalculateBlend(transformedTForBlend);

        return sign * ((1 - blendFactor) * func1 + blendFactor * func2);
    }
}