using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class SeaScooter : ActionBasedContinuousMoveProvider {
    [SerializeField] protected Transform rightHandTransform;


    [SerializeField]
    [Tooltip(
        "The Input System Action that will be used to read Move data from the right hand controller. Must be a Value Vector2 Control.")]
    protected InputActionProperty rightHandMoveAction =
        new InputActionProperty(new InputAction("Right Hand Move", expectedControlType: "Vector2"));

    private float _timeAccelerating = 0;

    [SerializeField] private float minSpeed;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float accelerationRate;

    [SerializeField] private float backSpeed;

    protected override void Awake() {
        base.Awake();

        forwardSource = rightHandTransform;
    }

    protected override Vector3 ComputeDesiredMove(Vector2 input) {
        if (input == Vector2.zero)
            return Vector3.zero;

        input = rightHandMoveAction.action?.ReadValue<Vector2>() ?? Vector2.zero;

        input.x = 0; // TODO: have this turn it a bit

        if (input.y < 0) {
            moveSpeed = backSpeed;

            _timeAccelerating = 0;
        }

        if (input.y == 0) {
            _timeAccelerating = 0;
        }

        if (input.y > 0) {
            _timeAccelerating += Time.deltaTime;

            moveSpeed = CalculateSpeed();
        }


        return base.ComputeDesiredMove(input);
    }

    private float CalculateSpeed() {
        return maxSpeed - (maxSpeed - minSpeed) * Mathf.Exp(-accelerationRate * _timeAccelerating);
    }
}