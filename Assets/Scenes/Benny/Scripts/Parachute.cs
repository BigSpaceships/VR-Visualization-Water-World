
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class Parachute : XRBaseInteractable
{
    [Header("Custom Fields")]
    [SerializeField] Transform selectedParent;
    [SerializeField] Transform objectParent;
    [SerializeField] int attachSpeed;
    [SerializeField] XRRayInteractor leftRay;
    [SerializeField] XRRayInteractor rightRay;
    [SerializeField] XRInteractorLineVisual leftRayVisual;
    [SerializeField] XRInteractorLineVisual rightRayVisual;
    [SerializeField] LayerMask colExclude;
    [SerializeField] InteractionLayerMask grabLayer;
    [SerializeField] InteractionLayerMask parachuteLayer;
    [SerializeField] float minReleaseForce;
    [SerializeField] float maxReleaseForce;
    [SerializeField] InputActionProperty leftGripProperty;
    [SerializeField] InputActionProperty rightGripProperty;
    [SerializeField] Color parachuteColor;
    public static SkiController leftSkiController;
    public static SkiController rightSkiController;
    SkiController skiController;
    IXRSelectInteractor selectInteractor;
    InputAction leftGrip;
    InputAction rightGrip;
    Collider[] cols;
    Rope[] ropes;
    int colCount;
    Transform myT;
    Rigidbody rb;
    Animator animator;
    Coroutine coroutine;

    protected override void Awake()
    {
        base.Awake();
        myT = transform;
        cols = GetComponentsInChildren<Collider>();
        colCount = cols.Length;
        rb = GetComponent<Rigidbody>();
        Transform model = myT.GetChild(0);
        animator = model.GetComponent<Animator>();
        Transform offset = model.GetChild(0);
        offset.GetChild(0).GetComponent<Renderer>().material.color = parachuteColor;
        ropes = new Rope[34];
        for (int i = 0; i < 34; i++) ropes[i] = offset.GetChild(i + 1).GetComponent<Rope>();
        leftGrip = leftGripProperty.action;
        rightGrip = rightGripProperty.action;
    }

    void Update()
    {
        //Both controllers can release parachute
        if (!isSelected) return;
        if (skiController == leftSkiController && rightGrip.WasPressedThisFrame()) interactionManager.SelectExit(selectInteractor, this);
        else if (skiController == rightSkiController && leftGrip.WasPressedThisFrame()) interactionManager.SelectExit(selectInteractor, this);
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        //Selected parachute
        base.OnSelectEntered(args);
        if (coroutine != null) StopCoroutine(coroutine);
        Pole pole = leftSkiController.attachedPole;
        if (pole != null) interactionManager.SelectExit(pole.selectInteractor, pole);
        pole = rightSkiController.attachedPole;
        if (pole != null) interactionManager.SelectExit(pole.selectInteractor, pole);
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.None;
        for (int i = 0; i < colCount; i++)
        {
            Collider col = cols[i];
            col.enabled = false;
            col.excludeLayers = colExclude;
        }
        leftRayVisual.enabled = rightRayVisual.enabled = false;
        leftRay.interactionLayers = rightRay.interactionLayers = parachuteLayer;
        selectInteractor = args.interactorObject;
        skiController = selectInteractor.transform.parent.GetComponent<SkiController>();
        myT.parent = selectedParent;
        animator.SetTrigger("Unfurl");
        Skier.paragliding = true;
        Skier.parachute = myT;
        Skier.rb.drag = 5;
        for (int i = 0; i < 34; i++) ropes[i].paragliding = true;
        coroutine = StartCoroutine(Selected(new Vector3(0, 1.65f, 0.5f), Quaternion.Euler(0, -90, 0)));
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        //Released parachute
        base.OnSelectExited(args);
        if (coroutine != null) StopCoroutine(coroutine);
        Skier.paragliding = false;
        for (int i = 0; i < 34; i++) ropes[i].paragliding = false;
        myT.parent = objectParent;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        animator.SetTrigger("Close");
        rb.AddRelativeForce(new Vector3(1, 0, Random.Range(-1f, 1f)) * Random.Range(minReleaseForce, maxReleaseForce), ForceMode.VelocityChange);
        rb.AddRelativeTorque(new Vector3(0, Random.Range(-1f, 1f), 0) * Random.Range(minReleaseForce, maxReleaseForce), ForceMode.VelocityChange);
        for (int i = 0; i < colCount; i++)
        {
            Collider col = cols[i];
            col.enabled = true;
            col.excludeLayers = 0;
        }
        leftRayVisual.enabled = rightRayVisual.enabled = true;
        leftRay.interactionLayers = rightRay.interactionLayers = grabLayer;
        leftSkiController.Animate("Deselect");
        rightSkiController.Animate("Deselect");
        coroutine = StartCoroutine(Deselected());
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
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1) yield return null;
        for (int i = 0; i < colCount; i++) cols[i].enabled = true;
        coroutine = null;
    }

    IEnumerator Deselected()
    {
        //Rapid grab queue handling
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1) yield return null;
        coroutine = null;
    }
}