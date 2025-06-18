using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Skis : XRBaseInteractable
{
    [Header("Custom Fields")]
    [SerializeField] float alignStrength;
    [SerializeField] LayerMask groundMask;
    [SerializeField] Transform skiParent;
    [SerializeField] Transform objectParent;
    [SerializeField] Vector3 leftAttach;
    [SerializeField] Vector3 rightAttach;
    [SerializeField] float minReleaseForce;
    [SerializeField] float maxReleaseForce;
    [SerializeField] Color skiColor;
    [SerializeField] int attachSpeed;
    Transform myT;
    Rigidbody rb;
    Collider col;
    bool selected;
    public static SkiController leftSkiController;
    public static SkiController rightSkiController;
    public static Transform leftController;
    public static Transform rightController;
    SkiController skiController;
    Coroutine coroutine;
    //bool colliding;
    //Vector3 collisionNormal;

    protected override void Awake()
    {
        base.Awake();
        myT = transform;
        rb = GetComponent<Rigidbody>();
        Transform model = myT.GetChild(0);
        col = model.GetComponent<Collider>();
        model.GetComponent<Renderer>().material.color = skiColor;
    }

    void Update()
    {
        if (!selected) return;

        //Ski stabilization
        if (Physics.Raycast(myT.position + myT.up * 0.5f, -myT.up, out RaycastHit hit, 0.6f, groundMask))
        {
            Quaternion targetRotation = Quaternion.FromToRotation(myT.up, hit.normal) * myT.rotation;
            myT.rotation = Quaternion.Slerp(myT.rotation, targetRotation, Mathf.Clamp01(Time.deltaTime * alignStrength));
        }
        else myT.localRotation = Quaternion.Slerp(myT.localRotation, Quaternion.identity, Mathf.Clamp01(Time.deltaTime * alignStrength));
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        //Which foot to put ski on
        if (coroutine != null) StopCoroutine(coroutine);
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
                    coroutine = null;
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
                    coroutine = null;
                    interactionManager.SelectEnter(interactorObject, this);
                    return;
                }
            }

            //Grabbing mechanics
            if (rb != null)
            {
                Destroy(rb);
                rb = null;
            }
            myT.parent = skiParent;
            Skier.attachedSkis++;
            if (Skier.attachedSkis == 1)
            {
                Skier.myT.position += new Vector3(0, 0.05f, 0);
                Rigidbody skierRb = Skier.rb;
                skierRb.freezeRotation = skierRb.useGravity = false;
                skierRb.drag = 1;
            }
            col.enabled = false;
            coroutine = StartCoroutine(Selected(attach, Quaternion.identity));
            interactionManager.SelectExit(interactorObject, this);
            selected = true;
        }
        else
        {
            //Releasing mechanics
            if (coroutine != null) StopCoroutine(coroutine);
            selected = false;
            col.enabled = true;
            skiController.attachedSki = null;
            Skier.attachedSkis--;
            if (Skier.attachedSkis == 0)
            {
                Rigidbody skierRb = Skier.rb;
                skierRb.freezeRotation = skierRb.useGravity = true;
                skierRb.drag = 3;
            }
            interactionManager.SelectExit(args.interactorObject, this);
            myT.parent = objectParent;
            if (rb == null)
            {
                rb = myT.AddComponent<Rigidbody>();
                rb = GetComponent<Rigidbody>();
            }
            rb.mass = 0.7f;
            rb.drag = rb.angularDrag = 1;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            if (skiController == leftSkiController) rb.AddRelativeForce(new Vector3(-1, 0, Random.Range(-1f, 1f)) * Random.Range(minReleaseForce, maxReleaseForce), ForceMode.VelocityChange);
            else rb.AddRelativeForce(new Vector3(1, 0, Random.Range(-1f, 1f)) * Random.Range(minReleaseForce, maxReleaseForce), ForceMode.VelocityChange);
            rb.AddRelativeTorque(new Vector3(0, Random.Range(-1f, 1f), 0) * Random.Range(minReleaseForce, maxReleaseForce), ForceMode.VelocityChange);
            coroutine = null;
        }
    }

    IEnumerator Selected(Vector3 targetPos, Quaternion targetRot)
    {
        //Force grab
        myT.GetLocalPositionAndRotation(out Vector3 pos, out Quaternion rot);
        while (!Manager.EqualVectors(pos, targetPos) || !Manager.EqualVectors(rot.eulerAngles, targetRot.eulerAngles))
        {
            pos = Vector3.Lerp(pos, targetPos, Time.deltaTime * attachSpeed);
            rot = Quaternion.Slerp(rot, targetRot, Time.deltaTime * attachSpeed);
            myT.SetLocalPositionAndRotation(pos, rot);
            yield return null;
        }
        myT.SetLocalPositionAndRotation(targetPos, targetRot);
        col.enabled = true;
        coroutine = null;
    }

    /*void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer != 9) return;
        colliding = true;
        //collisionNormal = collision.GetContact(0).normal;
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.layer != 9) return;
        if (!colliding) colliding = true;
        //collisionNormal = collision.GetContact(0).normal;
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer != 9) return;
        colliding = false;
    }*/
}
