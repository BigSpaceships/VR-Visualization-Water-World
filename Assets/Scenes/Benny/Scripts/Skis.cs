using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Skis : XRBaseInteractable
{
    [SerializeField] float alignStrength;
    [SerializeField] int maxAlignAngle;
    [SerializeField] LayerMask groundMask;
    [SerializeField] Transform skiParent;
    [SerializeField] Transform objectParent;
    [SerializeField] Vector3 leftAttach;
    [SerializeField] Vector3 rightAttach;
    [SerializeField] Transform leftController;
    [SerializeField] Transform rightController;
    [SerializeField] float minDeselectForce;
    [SerializeField] float maxDeselectForce;
    [SerializeField] Color skiColor;
    Transform myT;
    Rigidbody rb;
    bool selected;
    SkiController leftSkiController;
    SkiController rightSkiController;
    SkiController skiController;

    protected override void Awake()
    {
        base.Awake();
        myT = transform;
        rb = GetComponent<Rigidbody>();
        leftSkiController = leftController.GetComponent<SkiController>();
        rightSkiController = rightController.GetComponent<SkiController>();
        myT.GetChild(0).GetComponent<Renderer>().material.color = skiColor;
    }

    void Update()
    {
        if (!selected) return;

        //Ski stabilization
        if (Physics.Raycast(myT.position + myT.up * 0.5f, -myT.up, out RaycastHit hit, 0.6f, groundMask))
        {
            Vector3 groundUp = hit.normal;
            if (Vector3.Angle(myT.up, groundUp) < maxAlignAngle)
            {
                Quaternion targetRotation = Quaternion.FromToRotation(myT.up, groundUp) * myT.rotation;
                myT.rotation = Quaternion.Slerp(myT.rotation, targetRotation, Mathf.Clamp01(Time.deltaTime * alignStrength));
            }
        }
        else myT.localRotation = Quaternion.Slerp(myT.localRotation, Quaternion.identity, Mathf.Clamp01(Time.deltaTime * alignStrength));
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        //Which foot to put ski on
        base.OnSelectEntered(args);
        Vector3 attach = Vector3.zero;
        if (myT.parent == objectParent)
        {
            IXRSelectInteractor interactorObject = args.interactorObject;
            Transform interactor = interactorObject.transform.parent;
            if (interactor == leftController)
            {
                if (leftSkiController.attachedSki == null)
                {
                    leftSkiController.attachedSki = this;
                    attach = leftAttach;
                    skiController = leftSkiController;
                }
                else if (rightSkiController.attachedSki == null)
                {
                    rightSkiController.attachedSki = this;
                    attach = rightAttach;
                    skiController = rightSkiController;
                }
                else
                {
                    interactionManager.SelectEnter(interactorObject, leftSkiController.attachedSki);
                    interactionManager.SelectExit(interactorObject, this);
                    interactionManager.SelectEnter(interactorObject, this);
                    return;
                }
            }
            else if (interactor == rightController)
            {
                if (rightSkiController.attachedSki == null)
                {
                    rightSkiController.attachedSki = this;
                    attach = rightAttach;
                    skiController = rightSkiController;
                }
                else if (leftSkiController.attachedSki == null)
                {
                    leftSkiController.attachedSki = this;
                    attach = leftAttach;
                    skiController = leftSkiController;
                }
                else
                {
                    interactionManager.SelectEnter(interactorObject, rightSkiController.attachedSki);
                    interactionManager.SelectExit(interactorObject, this);
                    interactionManager.SelectEnter(interactorObject, this);
                    return;
                }
            }

            //Grabbing mechanics
            myT.parent = skiParent;
            myT.SetLocalPositionAndRotation(attach, Quaternion.identity);
            if (rb != null) Destroy(rb);
            Skier.attachedSkis++;
            if (Skier.attachedSkis == 1)
            {
                Skier.myT.position += new Vector3(0, 0.05f, 0);
                Skier.rb.freezeRotation = false;
                Skier.rb.drag = 0.1f;
            }
            interactionManager.SelectExit(args.interactorObject, this);
            selected = true;
        }
        else
        {
            //Releasing mechanics
            selected = false;
            skiController.attachedSki = null;
            Skier.attachedSkis--;
            if (Skier.attachedSkis == 0)
            {
                Skier.rb.freezeRotation = true;
                Skier.rb.drag = 3;
            }
            interactionManager.SelectExit(args.interactorObject, this);
            myT.parent = objectParent;
            rb = myT.AddComponent<Rigidbody>();
            if (rb == null) rb = GetComponent<Rigidbody>();
            rb.mass = 0.7f;
            rb.drag = rb.angularDrag = 1;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            if (skiController == leftSkiController) rb.AddRelativeForce(new Vector3(-1, 0, Random.value) * Random.Range(minDeselectForce, maxDeselectForce), ForceMode.VelocityChange);
            else rb.AddRelativeForce(new Vector3(1, 0, Random.value) * Random.Range(minDeselectForce, maxDeselectForce), ForceMode.VelocityChange);
            rb.AddRelativeTorque(new Vector3(0, Random.value, 1).normalized * Random.Range(minDeselectForce, maxDeselectForce), ForceMode.VelocityChange);
        }
    }
}
