using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Interaction.Toolkit;

public class Pole : XRBaseInteractable
{
    [SerializeField] Transform objectParent;
    [SerializeField] Transform skiParent;
    [SerializeField] Rigidbody skier;
    [SerializeField] Color poleColor;
    [SerializeField] float poleForce = 0.2f;
    [SerializeField] int maxPoleForce = 2;
    [SerializeField] Transform rightController;
    [SerializeField] Transform leftController;
    [SerializeField] LayerMask groundMask;
    public static Vector3 rightAttach;
    public static Vector3 leftAttach;
    SkiController rightSkiController;
    SkiController leftSkiController;
    SkiController skiController;
    Transform myT;
    Rigidbody rb;
    Collider[] cols;
    int colliderCount;
    Vector3 lastPos;
    Vector3 vel;
    float speed;
    WaitForSeconds velocityWait = new WaitForSeconds(0.1f);

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
        cols = GetComponentsInChildren<Collider>();
        colliderCount = cols.Length;
        myT = transform;
        rightSkiController = rightController.GetComponent<SkiController>();
        leftSkiController = leftController.GetComponent<SkiController>();
        myT.GetChild(1).GetComponent<Renderer>().material.color = poleColor;
        myT.GetChild(2).GetComponent<Renderer>().material.color = poleColor;
    }

    void Start()
    {
        StartCoroutine(CalculateVelocity());
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        //Grabbing mechanics
        base.OnSelectEntered(args);
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.None;
        for (int i = 0; i < colliderCount; i++) cols[i].enabled = false;
        if (args.interactorObject.transform.parent == rightController)
        {
            myT.parent = rightController;
            myT.SetLocalPositionAndRotation(rightAttach, new Quaternion(0, 0, -1, 0));
            skiController = rightSkiController;
        }
        else if (args.interactorObject.transform.parent == leftController)
        {
            myT.parent = leftController;
            myT.SetLocalPositionAndRotation(leftAttach, new Quaternion(0, 0, -1, 0));
            skiController = leftSkiController;
        }
        skiController.attachedPole = this;
        XRInteractorLineVisual ray = args.interactorObject.transform.GetComponent<XRInteractorLineVisual>();
        if (ray != null) ray.enabled = false;
        skiController.Animate("Select");
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        //Releasing mechanics
        base.OnSelectExited(args);
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        for (int i = 0; i < colliderCount; i++) cols[i].enabled = true;
        myT.parent = objectParent;
        skiController.attachedPole = null;
        XRInteractorLineVisual ray = args.interactorObject.transform.GetComponent<XRInteractorLineVisual>();
        if (ray != null) ray.enabled = true;
        skiController.Animate("Deselect");
    }

    public void SwitchController()
    {
        //Switch local position when toggling hands/controller
        if (skiController == rightSkiController) myT.localPosition = rightAttach;
        else if (skiController == leftSkiController) myT.localPosition = leftAttach;
    }

    void FixedUpdate()
    {
        //Pole push
        if (!isSelected || Skier.attachedSkis == 0) return;
        if (Physics.Raycast(myT.position - myT.up * 1.25f, myT.up, out RaycastHit hit, 2, groundMask) && speed > 10)
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
        //Calculate pole velocity for push mechanics
        while (true)
        {
            if (isSelected && Skier.attachedSkis > 0)
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
