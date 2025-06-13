using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Pole : XRBaseInteractable
{
    [SerializeField] Transform skiParent;
    [SerializeField] Rigidbody skier;
    [SerializeField] float poleForce = 0.2f;
    [SerializeField] int maxPoleForce = 2;
    [SerializeField] Transform rightController;
    [SerializeField] Transform leftController;
    [SerializeField] LayerMask groundMask;
    public static Vector3 rightAttach = new Vector3(0, -0.67f, 0.05f);
    Vector3 leftAttach;
    SkiController rightSkiController;
    SkiController leftSkiController;
    SkiController skiController;
    Transform myT;
    Rigidbody rb;
    Collider col;
    Vector3 lastPos;
    Vector3 vel;
    float speed;
    WaitForSeconds velocityWait = new WaitForSeconds(0.1f);

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        myT = transform;
        rightSkiController = rightController.GetComponent<SkiController>();
        leftSkiController = leftController.GetComponent<SkiController>();
    }

    void Start()
    {
        StartCoroutine(CalculateVelocity());
        leftAttach = rightAttach;
        leftAttach.x *= -1;
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.None;
        col.enabled = false;
        if (args.interactorObject.transform.parent == rightController)
        {
            myT.parent = rightController;
            myT.SetLocalPositionAndRotation(rightAttach, Quaternion.identity);
            skiController = rightSkiController;
        }
        else if (args.interactorObject.transform.parent == leftController)
        {
            myT.parent = leftController;
            myT.SetLocalPositionAndRotation(leftAttach, Quaternion.identity);
            skiController = leftSkiController;
        }
        XRInteractorLineVisual ray = args.interactorObject.transform.GetComponent<XRInteractorLineVisual>();
        if (ray != null) ray.enabled = false;
        skiController.Animate("Select");
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        col.enabled = true;
        myT.parent = null;
        XRInteractorLineVisual ray = args.interactorObject.transform.GetComponent<XRInteractorLineVisual>();
        if (ray != null) ray.enabled = true;
        skiController.Animate("Deselect");
    }

    void FixedUpdate()
    {
        if (!isSelected) return;
        if (Physics.Raycast(myT.position + myT.up * 0.7f, -myT.up, out RaycastHit hit, 1.6f, groundMask) && speed > 10)
        {
            if (hit.collider.gameObject.layer == 3) return;
            Vector3 groundUp = hit.normal;
            Vector3 poleDirection = Vector3.ProjectOnPlane(vel, groundUp).normalized;
            Vector3 skierDirection = Vector3.ProjectOnPlane(skiParent.forward, groundUp).normalized;
            float push = Vector3.Dot(poleDirection, skierDirection);
            if (push < -0.5) skier.AddForce(skiParent.forward * Mathf.Clamp(poleForce * speed, -maxPoleForce, maxPoleForce), ForceMode.VelocityChange);
        }
    }

    IEnumerator CalculateVelocity()
    {
        while (true)
        {
            if (isSelected)
            {
                Vector3 currentPos = myT.position - skiParent.position;
                vel = (currentPos - lastPos) * 10;
                speed = vel.sqrMagnitude;
                lastPos = currentPos;
                yield return velocityWait;
            }
            else yield return null;
        }
    }
}
