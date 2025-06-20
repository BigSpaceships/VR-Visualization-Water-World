using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Pole : XRBaseInteractable
{
    [Header("Custom Fields")]
    [SerializeField] Transform objectParent;
    [SerializeField] Transform skiParent;
    [SerializeField] Rigidbody skier;
    [SerializeField] Color poleColor;
    [SerializeField] float poleForce;
    [SerializeField] int maxPoleForce;
    [SerializeField] LayerMask groundMask;
    [SerializeField] int attachSpeed;
    [SerializeField] InteractionLayerMask grabLayer;
    [SerializeField] InteractionLayerMask poleLayer;
    [SerializeField] AudioClip snowHit;
    [SerializeField] AudioClip solidHit;
    [SerializeField] AudioClip grab;
    public static AudioSource effectSource;
    public static Vector3 rightAttach;
    public static Vector3 leftAttach;
    public IXRSelectInteractor selectInteractor;
    public static SkiController rightSkiController;
    public static SkiController leftSkiController;
    public static Transform rightController;
    public static Transform leftController;
    SkiController skiController;
    Transform myT;
    Rigidbody rb;
    Collider[] cols;
    Vector3 lastPos;
    Vector3 vel;
    float speed;
    WaitForSeconds velocityWait = new WaitForSeconds(0.1f);
    Coroutine coroutine;
    bool isGrounded = true;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
        myT = transform;
        cols = new Collider[] { myT.GetChild(0).GetComponent<Collider>(), myT.GetChild(1).GetComponent<Collider>(), myT.GetChild(2).GetComponent<Collider>() };
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
        if (coroutine != null) StopCoroutine(coroutine);
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.None;
        for (int i = 0; i < 3; i++) cols[i].enabled = false;
        selectInteractor = args.interactorObject;
        Transform interactorObject = selectInteractor.transform;
        Transform interactor = interactorObject.parent;
        if (interactor == rightController)
        {
            myT.parent = rightController;
            coroutine = StartCoroutine(Selected(rightAttach, new Quaternion(0, 0, -1, 0)));
            skiController = rightSkiController;
        }
        else if (interactor == leftController)
        {
            myT.parent = leftController;
            coroutine = StartCoroutine(Selected(leftAttach, new Quaternion(0, 0, -1, 0)));
            skiController = leftSkiController;
        }
        skiController.attachedPole = this;
        XRInteractorLineVisual ray = interactorObject.GetComponent<XRInteractorLineVisual>();
        if (ray != null)
        {
            interactorObject.GetComponent<XRRayInteractor>().interactionLayers = poleLayer;
            ray.enabled = false;
        }
        effectSource.PlayOneShot(grab);
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
        skiController.Animate("Select");
        coroutine = null;
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        //Releasing mechanics
        base.OnSelectExited(args);
        if (coroutine != null) StopCoroutine(coroutine);
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        for (int i = 0; i < 3; i++) cols[i].enabled = true;
        selectInteractor = null;
        myT.parent = objectParent;
        skiController.attachedPole = null;
        Transform interactorObject = args.interactorObject.transform;
        XRInteractorLineVisual ray = interactorObject.GetComponent<XRInteractorLineVisual>();
        if (ray != null)
        {
            interactorObject.GetComponent<XRRayInteractor>().interactionLayers = grabLayer;
            ray.enabled = true;
        }
        skiController.Animate("Deselect");
        coroutine = null;
        effectSource.PlayOneShot(grab);
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
        if (!isSelected || coroutine != null) return;
        if (Physics.Raycast(myT.position - myT.up * 1.25f, myT.up, out RaycastHit hit, 2, groundMask) && speed > 10)
        {
            if (!isGrounded)
            {
                isGrounded = true;
                if (hit.transform.gameObject.layer == 9) effectSource.PlayOneShot(snowHit);
                else effectSource.PlayOneShot(solidHit);
            }
            if (Skier.attachedSkis == 0) return;
            Vector3 groundUp = hit.normal;
            Vector3 poleDirection = Vector3.ProjectOnPlane(vel, groundUp).normalized;
            Vector3 skierDirection = Vector3.ProjectOnPlane(skiParent.forward, groundUp).normalized;
            float push = Vector3.Dot(poleDirection, skierDirection);
            if (push < -0.5) skier.AddForce(skiParent.forward * Mathf.Clamp(poleForce * speed, -maxPoleForce, maxPoleForce), ForceMode.VelocityChange);
        }
        else if (isGrounded) isGrounded = false;
    }

    IEnumerator CalculateVelocity()
    {
        //Calculate pole velocity for push mechanics
        while (true)
        {
            if (isSelected && Skier.attachedSkis > 0 && coroutine == null)
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

    void OnCollisionEnter(Collision collision)
    {
        if (!Skier.initialized) return;
        int layer = collision.gameObject.layer;
        if (layer == 9) effectSource.PlayOneShot(snowHit);
        else if (layer == 14) effectSource.PlayOneShot(solidHit);
    }
}
