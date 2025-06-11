using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Mathematics;
using UnityEngine;

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

    void Awake()
    {
        myT = transform;
        rb = GetComponent<Rigidbody>();
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
        Vector3 pos = cam.localPosition;
        cam.localPosition = new Vector3(0, pos.y, 0);
    }

    void LateUpdate()
    {
        skiSpeed = baseSkiSpeed - cam.localPosition.y * crouchSpeedIncrease;
        if (skiSpeed < 0) skiSpeed = 0;

        skiParent.rotation = cam.rotation;
        skiParent.localRotation = Quaternion.Euler(0, skiParent.localEulerAngles.y, 0);

        RaycastHit hit;
        Vector3 origin = rightSki.position + new Vector3(0, 0.5f, 0.7f);
        if (Physics.Raycast(origin, -rightSki.up, out hit, 2, groundMask))
        {
            Vector3 targetUp = hit.normal;
            if (Vector3.Angle(rightSki.up, targetUp) < maxAlignAngle)
            {
                Quaternion targetRotation = Quaternion.FromToRotation(rightSki.up, targetUp) * rightSki.rotation;
                rightSki.rotation = Quaternion.Slerp(rightSki.rotation, targetRotation, Time.deltaTime * skiAlignStrength);
            }
        }
        origin = leftSki.position + new Vector3(0, 0.5f, 0.7f);
        if (Physics.Raycast(origin, -leftSki.up, out hit, 2, groundMask))
        {
            Vector3 targetUp = hit.normal;
            if (Vector3.Angle(leftSki.up, targetUp) < maxAlignAngle)
            {
                Quaternion targetRotation = Quaternion.FromToRotation(leftSki.up, targetUp) * leftSki.rotation;
                leftSki.rotation = Quaternion.Slerp(leftSki.rotation, targetRotation, Time.deltaTime * skiAlignStrength);
            }
        }
    }

    void FixedUpdate()
    {
        Vector3 origin = rb.position;
        Vector3 currentUp = myT.up;
        if (Physics.Raycast(origin, -currentUp, out RaycastHit hit, 2, groundMask))
        {
            Vector3 targetUp = hit.normal;
            Vector3 rotationAxis = Vector3.Cross(currentUp, targetUp);
            float angle = Vector3.Angle(currentUp, targetUp);

            if (angle < maxAlignAngle) rb.AddTorque(rotationAxis.normalized * angle * alignStrength);

            Vector3 downhill = Vector3.ProjectOnPlane(Vector3.down, targetUp).normalized;
            Vector3 skierDirection = Vector3.ProjectOnPlane(cam.forward, targetUp).normalized;
            float alignment = Vector3.Dot(downhill, skierDirection);
            rb.AddForce(Vector3.Slerp(downhill, skierDirection, Mathf.Clamp01(Mathf.Clamp01(alignment) * skierDirectionWeight)) * skiSpeed, ForceMode.Acceleration);
            rb.drag = Mathf.Lerp(minDrag, maxDrag, 1 - Mathf.Clamp01(Mathf.Abs(alignment)));
        }
    }
}
