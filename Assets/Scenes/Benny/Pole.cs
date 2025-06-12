using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Pole : XRBaseInteractable
{
    [Header("Movement")]
    [SerializeField] Transform cam;
    [SerializeField] Rigidbody skier;
    [SerializeField] float poleForce = 0.2f;
    [SerializeField] int maxPoleForce = 2;
    [SerializeField] Transform rightController;
    [SerializeField] Transform leftController;
    [SerializeField] LayerMask groundMask;
    Transform myT;
    Rigidbody rb;
    Vector3 lastPos;
    Vector3 vel;
    float speed;
    WaitForSeconds velocityWait = new WaitForSeconds(0.1f);
    bool selected = false;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
        myT = transform;
    }

    void Start()
    {
        StartCoroutine(CalculateVelocity());
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.None;
        if (args.interactorObject.transform.parent == rightController)
        {
            myT.parent = rightController;
            myT.localPosition = new Vector3(0.25f, -0.7f, 0.5f);
        }
        else if (args.interactorObject.transform.parent == leftController)
        {
            myT.parent = leftController;
            myT.localPosition = new Vector3(-0.25f, -0.7f, 0.5f);
        }
        myT.localRotation = quaternion.identity;
        XRInteractorLineVisual ray = args.interactorObject.transform.GetComponent<XRInteractorLineVisual>();
        if (ray != null) ray.enabled = false;
        selected = true;
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        selected = false;
        base.OnSelectExited(args);
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        myT.parent = null;
        XRInteractorLineVisual ray = args.interactorObject.transform.GetComponent<XRInteractorLineVisual>();
        if (ray != null) ray.enabled = true;
    }

    void FixedUpdate()
    {
        if (!selected) return;
        if (Physics.Raycast(myT.position, -myT.up, out RaycastHit hit, 0.8f, groundMask) && speed > 10)
        {
            Vector3 groundUp = hit.normal;
            Vector3 poleDirection = Vector3.ProjectOnPlane(vel, groundUp).normalized;
            Vector3 skierDirection = Vector3.ProjectOnPlane(cam.forward, groundUp).normalized;
            float push = Vector3.Dot(poleDirection, skierDirection);
            if (push < -0.7) skier.AddRelativeForce(Vector3.forward * Mathf.Clamp(poleForce * speed, -maxPoleForce, maxPoleForce), ForceMode.VelocityChange);
        }
    }

    IEnumerator CalculateVelocity()
    {
        while (true)
        {
            if (selected)
            {
                Vector3 currentPos = myT.position - cam.position;
                vel = (currentPos - lastPos) * 10;
                speed = vel.sqrMagnitude;
                lastPos = currentPos;
                yield return velocityWait;
            }
            else yield return null;
        }
    }
}
