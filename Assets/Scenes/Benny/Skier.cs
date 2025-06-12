using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Mathematics;
using UnityEngine;
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

    [Header("Skis")]
    [SerializeField] Transform rightSki;
    [SerializeField] Transform leftSki;
    [SerializeField] int skiAlignStrength;
    Rigidbody rb;
    Transform myT;

    [Header("Poles")]
    [SerializeField] Rigidbody pole1;
    [SerializeField] Rigidbody pole2;
    [SerializeField] float poleForce;
    [SerializeField] float maxPoleForce;
    Transform pole1T;
    Transform pole2T;
    XRGrabInteractable pole1Grab;
    XRGrabInteractable pole2Grab;

    void Awake()
    {
        myT = transform;
        rb = GetComponent<Rigidbody>();
        pole1T = pole1.transform;
        pole2T = pole2.transform;
        pole1Grab = pole1.GetComponent<XRGrabInteractable>();
        pole2Grab = pole2.GetComponent<XRGrabInteractable>();
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
        cam.localPosition = new Vector3(0, 0, pos.y);
    }

    void LateUpdate()
    {
        //Camera-based logic
        skiSpeed = baseSkiSpeed - cam.localPosition.y * crouchSpeedIncrease;
        if (skiSpeed < 0) skiSpeed = 0;
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
        RaycastHit hit;

        //Skier stabilization and velocity control
        Vector3 origin = rb.position;
        Vector3 currentUp = myT.up;
        if (Physics.Raycast(origin, -currentUp, out hit, 2, groundMask))
        {
            Vector3 groundUp = hit.normal;
            Vector3 rotationAxis = Vector3.Cross(currentUp, groundUp);
            float angle = Vector3.Angle(currentUp, groundUp);

            if (angle < maxAlignAngle) rb.AddTorque(rotationAxis.normalized * angle * alignStrength);

            Vector3 downhill = Vector3.ProjectOnPlane(Vector3.down, groundUp).normalized;
            Vector3 skierDirection = Vector3.ProjectOnPlane(cam.forward, groundUp).normalized;
            float alignment = Vector3.Dot(downhill, skierDirection);
            rb.AddForce(Vector3.Slerp(downhill, skierDirection, Mathf.Clamp01(Mathf.Clamp01(alignment) * skierDirectionWeight)) * skiSpeed, ForceMode.Acceleration);
            rb.drag = Mathf.Lerp(minDrag, maxDrag, 1 - Mathf.Clamp01(Mathf.Abs(alignment)));
        }

        //Pole movement
        if (Physics.Raycast(pole1.position, -pole1T.up, out hit, 0.7f, groundMask) && pole1.velocity.sqrMagnitude > 100 && pole1Grab.isSelected)
        {
            Vector3 groundUp = hit.normal;
            Vector3 poleDirection = Vector3.ProjectOnPlane(pole1.velocity, groundUp).normalized;
            Vector3 skierDirection = Vector3.ProjectOnPlane(cam.forward, groundUp).normalized;
            float push = Vector3.Dot(poleDirection, skierDirection);
            if (push < -0.5) rb.AddRelativeForce(Vector3.back * Mathf.Clamp(push * poleForce, -maxPoleForce, maxPoleForce), ForceMode.VelocityChange);
        }
        if (Physics.Raycast(pole2.position, -pole2T.up, out hit, 0.7f, groundMask) && pole2.velocity.sqrMagnitude > 100 && pole2Grab.isSelected)
        {
            Vector3 groundUp = hit.normal;
            Vector3 poleDirection = Vector3.ProjectOnPlane(pole2.velocity, groundUp).normalized;
            Vector3 skierDirection = Vector3.ProjectOnPlane(cam.forward, groundUp).normalized;
            float push = Vector3.Dot(poleDirection, skierDirection);
            if (push < -0.5) rb.AddRelativeForce(Vector3.back * Mathf.Clamp(push * poleForce, -maxPoleForce, maxPoleForce), ForceMode.VelocityChange);
        }
    }
}
