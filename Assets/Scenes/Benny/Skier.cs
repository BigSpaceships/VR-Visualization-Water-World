using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Tweenables.Primitives;

public class Skier : MonoBehaviour
{
    [Header("Skier")]
    [SerializeField] Transform skiParent;
    [SerializeField] Transform cam;
    [SerializeField] int alignStrength;
    [SerializeField] int maxAlignAngle;
    [SerializeField] int baseSkiSpeed;
    [SerializeField] float crouchSpeedIncrease;
    float skiSpeed;
    [SerializeField] float skierDirectionWeight;
    [SerializeField] float minDrag;
    [SerializeField] int maxDrag;
    [SerializeField] LayerMask groundMask;

    [Header("Movement")]
    [SerializeField] int xMoveForce;
    [SerializeField] int zMoveForce;
    [SerializeField] InputActionProperty moveActionProperty;
    InputAction moveAction;
    [SerializeField] int turnForce;
    [SerializeField] InputActionProperty turnActionProperty;
    InputAction turnAction;
    [SerializeField] int jumpForce;
    [SerializeField] int flipForce;
    [SerializeField] InputActionProperty jumpActionProperty;
    InputAction jumpAction;
    bool isGrounded = true;
    bool jump;
    [SerializeField] InputActionProperty flipActionProperty;
    InputAction flipAction;
    bool flip;
    bool changedFlipAxis;
    Vector3 flipAxis;

    [Header("Skis")]
    [SerializeField] Transform rightSki;
    [SerializeField] Transform leftSki;
    [SerializeField] int skiAlignStrength;
    Rigidbody rb;
    Transform myT;

    void Awake()
    {
        myT = transform;
        rb = GetComponent<Rigidbody>();
        moveAction = moveActionProperty.action;
        turnAction = turnActionProperty.action;
        jumpAction = jumpActionProperty.action;
        flipAction = flipActionProperty.action;
    }

    void OnEnable()
    {
        Application.onBeforeRender += OnBeforeRender;
    }

    void OnDisable()
    {
        Application.onBeforeRender -= OnBeforeRender;
    }

    void OnBeforeRender()
    {
        //Fix camera
        Vector3 pos = cam.localPosition;
        cam.localPosition = new Vector3(0, pos.y, 0);
    }

    void Update()
    {
        //Ski faster when crouched
        skiSpeed = baseSkiSpeed - cam.localPosition.y * crouchSpeedIncrease;
        if (skiSpeed < 0) skiSpeed = 0;

        //Input detection
        if (isGrounded)
        {
            if (jumpAction.WasPressedThisFrame()) jump = true;
        }
        else
        {
            if (flipAction.IsPressed()) flip = true;
            else if (flipAction.WasReleasedThisFrame()) flip = false;
        }
    }

    void LateUpdate()
    {
        //Skis follow camera rotation
        skiParent.rotation = cam.rotation;
        skiParent.localRotation = Quaternion.Euler(0, skiParent.localEulerAngles.y, 0);

        //Ski stabilization
        RaycastHit hit;
        Vector3 origin = rightSki.position + new Vector3(0, 0.5f, 0.7f);
        if (Physics.Raycast(origin, -rightSki.up, out hit, 2, groundMask))
        {
            Vector3 groundUp = hit.normal;
            if (Vector3.Angle(rightSki.up, groundUp) < maxAlignAngle)
            {
                Quaternion targetRotation = Quaternion.FromToRotation(rightSki.up, groundUp) * rightSki.rotation;
                rightSki.rotation = Quaternion.Slerp(rightSki.rotation, targetRotation, Time.deltaTime * skiAlignStrength);
            }
        }
        origin = leftSki.position + new Vector3(0, 0.5f, 0.7f);
        if (Physics.Raycast(origin, -leftSki.up, out hit, 2, groundMask))
        {
            Vector3 groundUp = hit.normal;
            if (Vector3.Angle(leftSki.up, groundUp) < maxAlignAngle)
            {
                Quaternion targetRotation = Quaternion.FromToRotation(leftSki.up, groundUp) * leftSki.rotation;
                leftSki.rotation = Quaternion.Slerp(leftSki.rotation, targetRotation, Time.deltaTime * skiAlignStrength);
            }
        }
    }

    void FixedUpdate()
    {
        //Move/Turn
        Vector2 input = turnAction.ReadValue<Vector2>();
        rb.AddTorque(cam.TransformDirection(new Vector3(-input.y, input.x, 0)) * turnForce, ForceMode.Acceleration);
        input = moveAction.ReadValue<Vector2>();
        rb.AddForce(cam.TransformDirection(new Vector3(input.x * xMoveForce, 0, input.y * zMoveForce)), ForceMode.Acceleration);

        //Grounded raycast
        Vector3 currentUp = myT.up;
        if (Physics.Raycast(myT.position, -currentUp, out RaycastHit hit, 1.4f, groundMask))
        {
            //Skier stabilization
            Vector3 groundUp = hit.normal;
            Vector3 rotationAxis = Vector3.Cross(currentUp, groundUp);
            float angle = Vector3.Angle(currentUp, groundUp);
            if (angle < maxAlignAngle) rb.AddTorque(rotationAxis.normalized * angle * alignStrength);

            //Velocity control based on direction
            Vector3 downhill = Vector3.ProjectOnPlane(Vector3.down, groundUp).normalized;
            Vector3 skierDirection = Vector3.ProjectOnPlane(cam.forward, groundUp).normalized;
            float alignment = Vector3.Dot(downhill, skierDirection);
            rb.AddForce(Vector3.Slerp(downhill, skierDirection, Mathf.Clamp01(Mathf.Clamp01(alignment) * skierDirectionWeight)) * skiSpeed, ForceMode.Acceleration);
            rb.drag = Mathf.Lerp(minDrag, maxDrag, 1 - Mathf.Clamp01(Mathf.Abs(alignment)));

            //Jump
            isGrounded = true;
            if (jump)
            {
                rb.AddForce(cam.up * jumpForce, ForceMode.VelocityChange);
                if (!changedFlipAxis)
                {
                    flipAxis = UnityEngine.Random.value < 0.5f ? Vector3.forward : Vector3.back;
                    changedFlipAxis = true;
                }
                jump = false;
            }
            if (flip) flip = false;
        }
        else
        {
            //Flip
            isGrounded = false;
            if (jump) jump = false;
            if (flip)
            {
                rb.AddTorque(cam.TransformDirection(flipAxis) * flipForce, ForceMode.Acceleration);
                changedFlipAxis = false;
            }
        }
    }
}
