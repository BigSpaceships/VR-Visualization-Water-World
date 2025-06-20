using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Skier : MonoBehaviour
{
    [Header("Skier")]
    [SerializeField] Transform camOffset;
    [SerializeField] Transform cam;
    Transform interactableParent;
    [SerializeField] int alignStrength;
    float skiSpeed;
    [SerializeField] int skiGravity;
    [SerializeField] float minDrag;
    [SerializeField] int maxDrag;
    [SerializeField] float perpDeceleration;
    [SerializeField] LayerMask groundMask;
    public static int attachedSkis;
    [SerializeField] Transform leftController;
    [SerializeField] Transform rightController;
    SkiController leftSkiController;
    SkiController rightSkiController;
    [SerializeField] bool hands = true;

    [Header("Movement")]
    [SerializeField] float normalMoveForce;
    [SerializeField] int xMoveForce;
    [SerializeField] int zMoveForce;
    [SerializeField] int airMoveForce;
    [SerializeField] InputActionProperty moveActionProperty;
    InputAction moveAction;
    [SerializeField] int normalTurnForce;
    [SerializeField] int groundTurnForce;
    [SerializeField] int airTurnForce;
    [SerializeField] InputActionProperty turnActionProperty;
    InputAction turnAction;
    [SerializeField] InputActionProperty crouchActionProperty;
    InputAction crouchAction;
    [SerializeField] int baseSkiSpeed;
    [SerializeField] int crouchHeight;
    [SerializeField] float crouchSpeed;
    [SerializeField] int crouchSpeedIncrease;

    [Header("Jump")]
    [SerializeField] int normalJumpForce;
    [SerializeField] int jumpForce;
    [SerializeField] int paraglideJumpForce;
    [SerializeField] int flipForce;
    [SerializeField] InputActionProperty jumpActionProperty;
    InputAction jumpAction;
    bool isGrounded = true;
    bool colliding;
    //Vector3 collisionNormal;
    bool jump;
    [SerializeField] InputActionProperty leftFlipActionProperty;
    [SerializeField] InputActionProperty rightFlipActionProperty;
    InputAction leftFlipAction;
    InputAction rightFlipAction;
    bool leftFlip;
    bool rightFlip;
    [SerializeField] InputActionProperty resetActionProperty;
    InputAction resetAction;
    [SerializeField] InputActionProperty toggleControllerProperty;
    InputAction toggleController;
    [SerializeField] GameObject devSim;
    public static Rigidbody rb;
    public static Transform myT;
    UnityEngine.XR.InputDevice rightDevice;
    UnityEngine.XR.InputDevice leftDevice;
    //bool rightHaptics;
    //bool leftHaptics;

    [Header("Paraglider")]
    public static bool paragliding;
    public static Transform parachute;
    [SerializeField] Transform leftHandRopeTarget;
    [SerializeField] Transform rightHandRopeTarget;
    [SerializeField] Transform leftControllerRopeTarget;
    [SerializeField] Transform rightControllerRopeTarget;
    [SerializeField] int parachuteAdjustmentSpeed;
    [SerializeField] int paraglideGravity;
    WaitForSeconds updraftWait = new WaitForSeconds(0.3f);
    [SerializeField] int glideForce;
    [SerializeField] int inputGlideForce;
    [SerializeField] float paraglideTurnForce;
    [SerializeField] float parachuteVelocityOffset;
    Vector3 leftVel, rightVel, leftLastPos, rightLastPos;
    float leftSpeed, rightSpeed;
    WaitForSeconds velocityWait = new WaitForSeconds(0.1f);
    [SerializeField] Transform ringParent;
    Ring[] rings = new Ring[16];

    [Header("UI")]
    [SerializeField] Manager manager;
    CanvasGroup canvasGroup, resetCanvas;
    TMP_Text timeText, altText, speedText;
    public static TMP_Text ringText;
    public static int passedRings;
    int minutes;
    float seconds;
    Vector3 initialPos;
    Quaternion initialRot;
    Coroutine reload;
    WaitForSeconds oneSecond = new WaitForSeconds(1);
    public static bool initialized;

    void Awake()
    {
        myT = transform;
        rb = GetComponent<Rigidbody>();
        Skis.leftController = Pole.leftController = leftController;
        Skis.rightController = Pole.rightController = rightController;
        Skis.leftSkiController = Pole.leftSkiController = Parachute.leftSkiController = Rope.leftController = leftSkiController = leftController.GetComponent<SkiController>();
        Skis.rightSkiController = Pole.rightSkiController = Parachute.rightSkiController = Rope.rightController = rightSkiController = rightController.GetComponent<SkiController>();
        if (hands)
        {
            Rope.leftTarget = leftHandRopeTarget;
            Rope.rightTarget = rightHandRopeTarget;
            Pole.leftAttach = new Vector3(-0.035f, -1.28f, -0.03f);
            Pole.rightAttach = new Vector3(0.035f, -1.28f, -0.03f);
            leftSkiController.SwitchController(true);
            rightSkiController.SwitchController(true);
        }
        else
        {
            Rope.leftTarget = leftControllerRopeTarget;
            Rope.rightTarget = rightControllerRopeTarget;
            Pole.rightAttach = Pole.leftAttach = new Vector3(0, -1.3f, 0.05f);
            leftSkiController.SwitchController(false);
            rightSkiController.SwitchController(false);
        }
        interactableParent = myT.GetChild(0);
        for (int i = 0; i < 16; i++) rings[i] = ringParent.GetChild(i).GetComponent<Ring>();
        moveAction = moveActionProperty.action;
        turnAction = turnActionProperty.action;
        crouchAction = crouchActionProperty.action;
        jumpAction = jumpActionProperty.action;
        leftFlipAction = leftFlipActionProperty.action;
        rightFlipAction = rightFlipActionProperty.action;
        resetAction = resetActionProperty.action;
        toggleController = toggleControllerProperty.action;

        canvasGroup = manager.canvasGroup;
        resetCanvas = cam.GetChild(0).GetComponent<CanvasGroup>();
        resetCanvas.alpha = 0;
        Transform statCanvas = cam.GetChild(1);
        timeText = statCanvas.GetChild(0).GetComponent<TMP_Text>();
        altText = statCanvas.GetChild(1).GetComponent<TMP_Text>();
        speedText = statCanvas.GetChild(2).GetComponent<TMP_Text>();
        ringText = statCanvas.GetChild(3).GetComponent<TMP_Text>();
        ringText.enabled = false;
        myT.GetPositionAndRotation(out initialPos, out initialRot);
        altText.SetText("Altitude: " + (initialPos.y * 3.281f).ToString("0") + "ft");

#if UNITY_EDITOR
        Instantiate(devSim);
#endif
    }

    void Start()
    {
        camOffset.localPosition = new Vector3(0, 1.7f, 0);
        /*rightDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        if (rightDevice.isValid && rightDevice.TryGetHapticCapabilities(out HapticCapabilities capabilities) && capabilities.supportsImpulse) rightHaptics = true;
        else rightHaptics = false;
        leftDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        if (leftDevice.isValid && leftDevice.TryGetHapticCapabilities(out capabilities) && capabilities.supportsImpulse) leftHaptics = true;
        else leftHaptics = false;*/
        StartCoroutine(CalculateVelocity());
    }

    void Update()
    {
        if (!initialized) return;

        //Input detection
        if (isGrounded)
        {
            if (jumpAction.WasPressedThisFrame()) jump = true;
        }
        else
        {
            if (leftFlipAction.IsPressed() && attachedSkis > 0 && !paragliding) leftFlip = true;
            if (rightFlipAction.IsPressed() && attachedSkis > 0 && !paragliding) rightFlip = true;
        }
        if (leftFlipAction.WasReleasedThisFrame()) leftFlip = false;
        if (rightFlipAction.WasReleasedThisFrame()) rightFlip = false;

        if (resetAction.WasPressedThisFrame()) StartCoroutine(Reload(0.5f));
        if (toggleController.WasPressedThisFrame())
        {
            hands = !hands;
            if (hands)
            {
                Rope.leftTarget = leftHandRopeTarget;
                Rope.rightTarget = rightHandRopeTarget;
                Pole.rightAttach = new Vector3(0.035f, -1.28f, -0.03f);
                Pole.leftAttach = new Vector3(-0.035f, -1.28f, -0.03f);
                leftSkiController.SwitchController(true, paragliding);
                rightSkiController.SwitchController(true, paragliding);
            }
            else
            {
                Rope.leftTarget = leftControllerRopeTarget;
                Rope.rightTarget = rightControllerRopeTarget;
                Pole.rightAttach = Pole.leftAttach = new Vector3(0, -1.3f, 0.05f);
                leftSkiController.SwitchController(false);
                rightSkiController.SwitchController(false);
            }
        }

        //Crouch mechanics
        float height = camOffset.localPosition.y;
        if (crouchAction.IsPressed() && (!paragliding || isGrounded))
        {
            skiSpeed = baseSkiSpeed + crouchSpeedIncrease;
            if (height != crouchHeight) height = Mathf.Lerp(height, crouchHeight, Mathf.Clamp01(Time.deltaTime * crouchSpeed));
        }
        else
        {
            skiSpeed = baseSkiSpeed;
            if (height != 1.7f) height = Mathf.Lerp(height, 1.7f, Mathf.Clamp01(Time.deltaTime * crouchSpeed));
        }
        camOffset.localPosition = new Vector3(0, height, 0);

        Vector3 pos = myT.position;
        if (Mathf.Abs(pos.x) >= 1500 || Mathf.Abs(pos.z) >= 1500)
        {
            StartCoroutine(Reload(0.5f));
            return;
        }
        if (reload == null)
        {
            if (Mathf.Abs(pos.y) >= 1500) reload = StartCoroutine(ReloadCanvas(0.5f));
            else if (paragliding && passedRings == 16) reload = StartCoroutine(ReloadCanvas(0.5f, true));
            else
            {
                float posMag = new Vector2(pos.x, pos.z).sqrMagnitude;
                if (posMag >= 1210000) reload = StartCoroutine(ReloadCanvas(0.5f));
            }
        }

        //UI
        seconds += Time.deltaTime;
        if (Mathf.Round(seconds) >= 60)
        {
            seconds = 0;
            minutes++;
        }
        timeText.SetText("Time: " + minutes + ":" + seconds.ToString("00"));
        altText.SetText("Altitude: " + (myT.position.y * 3.281f).ToString("0") + "ft");
        speedText.SetText("Speed: " + (rb.velocity.magnitude * 3.281).ToString("0.0") + "ft/s");
    }

    void LateUpdate()
    {
        //Interactables follow camera rotation
        interactableParent.rotation = cam.rotation;
        interactableParent.localRotation = Quaternion.Euler(0, interactableParent.localEulerAngles.y, 0);
    }

    void FixedUpdate()
    {
        if (!initialized) return;
        
        //Movement input
        Vector2 turnInput = turnAction.ReadValue<Vector2>();
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        Vector3 currentUp = myT.up;
        RaycastHit hit;

        //Paragliding mechanics
        if (paragliding)
        {
            //Parachute adjustment
            Vector3 leftPos = interactableParent.InverseTransformPoint(leftController.position);
            Vector3 rightPos = interactableParent.InverseTransformPoint(rightController.position);
            float roll = Vector3.SignedAngle(Vector3.right, Vector3.ProjectOnPlane(rightPos - leftPos, Vector3.forward).normalized, Vector3.forward);
            roll = Mathf.Clamp(roll / 2, -45, 45);
            if (Mathf.Abs(roll) < 5) roll = 0;
            float t = Mathf.Clamp01(parachuteAdjustmentSpeed * Time.fixedDeltaTime);
            parachute.GetLocalPositionAndRotation(out Vector3 pos, out Quaternion qRot);
            Vector3 rot = qRot.eulerAngles;
            Vector3 targetPos = (leftPos + rightPos) / 2 + new Vector3(0, 0.1f + rb.velocity.y * parachuteVelocityOffset, 0.2f);
            targetPos.y = Mathf.Clamp(targetPos.y, 1.4f, 2.4f);
            parachute.SetLocalPositionAndRotation(Vector3.Lerp(pos, targetPos, t), Quaternion.Slerp(qRot, Quaternion.Euler(new Vector3(roll, rot.y, 0)), t));

            //Is grounded
            if (Physics.Raycast(myT.position + currentUp * 1.7f, -currentUp, out hit, 1.8f, groundMask) && colliding)
            {
                //Regular paragliding
                if (attachedSkis == 0)
                {
                    if (!isGrounded)
                    {
                        isGrounded = rb.useGravity = rb.freezeRotation = true;
                        rb.drag = 3;
                    }
                    Quaternion myRot = myT.rotation;
                    myT.rotation = Quaternion.Slerp(myRot, myRot * Quaternion.Euler(0, turnAction.ReadValue<Vector2>().x * normalTurnForce, 0), Time.fixedDeltaTime);
                    rb.AddForce(interactableParent.TransformDirection(new Vector3(moveInput.x, 0, moveInput.y)).normalized * normalMoveForce, ForceMode.VelocityChange);
                }
                //Parachute skiing (speedrunning)
                else
                {
                    if (!isGrounded)
                    {
                        isGrounded = true;
                        rb.useGravity = false;
                    }
                    rb.AddForce(Vector3.down * skiGravity);
                    //Skier stabilization
                    Vector3 groundUp = hit.normal;
                    Vector3 rotationAxis = Vector3.Cross(currentUp, groundUp);
                    rb.AddTorque(rotationAxis.normalized * Vector3.Angle(currentUp, groundUp) * alignStrength);
                    if (hit.collider.gameObject.layer != 14)
                    {
                        //Velocity control based on direction
                        Vector3 downhill = Vector3.ProjectOnPlane(Vector3.down, groundUp).normalized;
                        Vector3 skierDirection = Vector3.ProjectOnPlane(interactableParent.forward, groundUp).normalized;
                        float alignment = Vector3.Dot(downhill, skierDirection);
                        rb.AddForce(skierDirection * skiSpeed * Mathf.Clamp01(alignment + 1), ForceMode.Acceleration);
                        float targetDrag = Mathf.Lerp(minDrag, maxDrag, 1 - Mathf.Clamp01(Mathf.Abs(alignment) * 1.5f));
                        rb.drag = Mathf.Lerp(rb.drag, targetDrag, perpDeceleration);
                    }
                    else rb.drag = 3;
                    rb.AddForce(interactableParent.TransformDirection(new Vector3(moveInput.x * xMoveForce, 0, moveInput.y * zMoveForce)), ForceMode.Acceleration);
                    rb.AddTorque(interactableParent.TransformDirection(new Vector3(0, turnInput.x, 0)) * groundTurnForce, ForceMode.Acceleration);
                }
                if (jump)
                {
                    if (rb.freezeRotation) rb.freezeRotation = false;
                    rb.AddForce(interactableParent.up * paraglideJumpForce, ForceMode.VelocityChange);
                    StartCoroutine(Updraft(3000, interactableParent.TransformDirection(new Vector3(0, 1, -1)).normalized));
                    jump = false;
                }
            }
            //Is tilted on ground
            else if (colliding)
            {
                if (attachedSkis > 0)
                {
                    if (!isGrounded)
                    {
                        isGrounded = true;
                        rb.useGravity = false;
                        rb.freezeRotation = true;
                        rb.drag = minDrag;
                    }
                    rb.AddForce(Vector3.down * skiGravity);
                    rb.AddForce(interactableParent.TransformDirection(new Vector3(moveInput.x * xMoveForce, 0, moveInput.y * zMoveForce)), ForceMode.Acceleration);
                    rb.AddTorque(interactableParent.TransformDirection(new Vector3(-turnInput.y, turnInput.x, 0)).normalized * groundTurnForce, ForceMode.Acceleration);
                }
                else if (!isGrounded)
                {
                    isGrounded = rb.useGravity = true;
                    rb.freezeRotation = true;
                    rb.drag = 3;
                }
                if (jump) jump = false;
            }
            //Is in air
            else
            {
                if (jump) jump = false;
                if (isGrounded)
                {
                    isGrounded = rb.useGravity = rb.freezeRotation = false;
                    rb.drag = 5;
                }

                //Gliding/steering
                Vector3 rotationAxis = Vector3.Cross(currentUp, Vector3.up);
                rb.AddTorque(rotationAxis.normalized * Vector3.Angle(currentUp, Vector3.up) * alignStrength);
                float distance = Vector3.Distance(leftPos, rightPos);
                if (distance < 0.02f) distance = 0.02f;
                Vector3 scale = parachute.localScale;
                parachute.localScale = new Vector3(scale.x, scale.y, Mathf.Lerp(scale.z, Mathf.Clamp(distance * 5, 0.5f, 1.2f), t));
                rb.AddForce(Vector3.down * paraglideGravity * Mathf.Clamp(Mathf.Pow(0.2f / distance, 2), 0.2f, 10));
                rb.AddForce(interactableParent.forward * glideForce, ForceMode.Acceleration);
                rb.AddForce(interactableParent.TransformDirection(new Vector3(moveInput.x, 0, moveInput.y) + new Vector3(turnInput.x, 0, turnInput.y)).normalized * inputGlideForce, ForceMode.Acceleration);
                float startValue = rot.x;
                if (startValue > 180) startValue -= 360;
                rb.AddTorque(-interactableParent.up * Mathf.Lerp(startValue, roll, t) * paraglideTurnForce, ForceMode.Acceleration);
                if (leftSpeed > 1 && rightSpeed > 1 && Vector3.Dot(leftVel.normalized, interactableParent.up) < -0.7f && Vector3.Dot(rightVel.normalized, interactableParent.up) < -0.7f)
                {
                    rb.AddForce(interactableParent.up * (leftSpeed + rightSpeed) * 40, ForceMode.Acceleration);
                }
            }
            if (rightFlip) rightFlip = false;
            if (leftFlip) leftFlip = false;
            return;
        }

        //No skis
        if (attachedSkis == 0)
        {
            if (!rb.freezeRotation)
            {
                rb.freezeRotation = rb.useGravity = true;
                rb.drag = minDrag;
            }
            rb.AddForce(interactableParent.TransformDirection(new Vector3(moveInput.x, 0, moveInput.y).normalized * normalMoveForce), ForceMode.VelocityChange);
            if (Physics.Raycast(myT.position + currentUp * 1.7f, -currentUp, 1.75f, groundMask) && colliding)
            {
                if (!isGrounded)
                {
                    isGrounded = true;
                    if (rb.drag == minDrag) rb.drag = 3;
                }
                Quaternion rot = myT.rotation;
                myT.rotation = Quaternion.Slerp(rot, rot * Quaternion.Euler(0, turnAction.ReadValue<Vector2>().x * normalTurnForce, 0), Time.fixedDeltaTime);
                if (jump)
                {
                    rb.AddForce(interactableParent.up * normalJumpForce, ForceMode.VelocityChange);
                    jump = false;
                }
            }
            else
            {
                if (colliding && !isGrounded) isGrounded = true;
                if (jump) jump = false;
            }
            if (rightFlip) rightFlip = false;
            if (leftFlip) leftFlip = false;
            return;
        }

        //Attached skis
        rb.AddForce(Vector3.down * skiGravity);
        if (Physics.Raycast(myT.position + currentUp * 1.7f, -currentUp, out hit, 1.9f, groundMask))
        {
            if (!isGrounded) isGrounded = true;

            //Skier stabilization
            Vector3 groundUp = hit.normal;
            Vector3 rotationAxis = Vector3.Cross(currentUp, groundUp);
            rb.AddTorque(rotationAxis.normalized * Vector3.Angle(currentUp, groundUp) * alignStrength);
            if (hit.collider.gameObject.layer != 14)
            {
                //Velocity control based on direction
                Vector3 downhill = Vector3.ProjectOnPlane(Vector3.down, groundUp).normalized;
                Vector3 skierDirection = Vector3.ProjectOnPlane(interactableParent.forward, groundUp).normalized;
                float alignment = Vector3.Dot(downhill, skierDirection);
                rb.AddForce(skierDirection * skiSpeed * Mathf.Clamp01(alignment + 1), ForceMode.Acceleration);
                float targetDrag = Mathf.Lerp(minDrag, maxDrag, 1 - Mathf.Clamp01(Mathf.Abs(alignment) * 1.5f));
                rb.drag = Mathf.Lerp(rb.drag, targetDrag, perpDeceleration);
            }
            else rb.drag = 3;
            rb.AddTorque(interactableParent.TransformDirection(new Vector3(0, turnInput.x, 0)).normalized * (groundTurnForce + crouchSpeedIncrease), ForceMode.Acceleration);
            rb.AddForce(interactableParent.TransformDirection(new Vector3(moveInput.x * (xMoveForce + crouchSpeedIncrease), 0, moveInput.y * (zMoveForce + crouchSpeedIncrease))), ForceMode.Acceleration);
            if (jump)
            {
                rb.AddForce(interactableParent.up * jumpForce, ForceMode.VelocityChange);
                jump = false;
            }
            if (rightFlip) rightFlip = false;
            if (leftFlip) leftFlip = false;
        }
        else if (colliding)
        {
            //Is tilted on ground
            if (!isGrounded) isGrounded = true;
            rb.AddTorque(interactableParent.TransformDirection(new Vector3(-turnInput.y, turnInput.x, 0)).normalized * (groundTurnForce + crouchSpeedIncrease), ForceMode.Acceleration);
            rb.AddForce(interactableParent.TransformDirection(new Vector3(moveInput.x * (xMoveForce + crouchSpeedIncrease), 0, moveInput.y * (zMoveForce + crouchSpeedIncrease))), ForceMode.Acceleration);
            if (jump) jump = false;
            if (rightFlip) rightFlip = false;
            if (leftFlip) leftFlip = false;
        }
        else
        {
            //Is in air
            if (isGrounded) isGrounded = false;
            rb.drag = minDrag;
            rb.AddTorque(interactableParent.TransformDirection(new Vector3(-turnInput.y, turnInput.x, 0)).normalized * airTurnForce, ForceMode.Acceleration);
            rb.AddForce(interactableParent.TransformDirection(new Vector3(moveInput.x, 0, moveInput.y)).normalized * airMoveForce, ForceMode.Acceleration);
            if (jump) jump = false;
            if (leftFlip) rb.AddTorque(interactableParent.TransformDirection(Vector3.forward) * flipForce, ForceMode.Acceleration);
            if (rightFlip) rb.AddTorque(interactableParent.TransformDirection(Vector3.back) * flipForce, ForceMode.Acceleration);
        }
    }

    IEnumerator Updraft(float updraftForce, Vector3 direction)
    {
        yield return updraftWait;
        rb.AddForce(direction * updraftForce, ForceMode.Acceleration);
    }

    IEnumerator CalculateVelocity()
    {
        //Calculate controller velocity for paragliding
        while (true)
        {
            if (paragliding && !isGrounded && !colliding)
            {
                Vector3 leftCurrentPos = leftController.localPosition;
                leftVel = (leftCurrentPos - leftLastPos) * 10;
                leftSpeed = leftVel.sqrMagnitude;
                Vector3 rightCurrentPos = rightController.localPosition;
                rightVel = (rightCurrentPos - rightLastPos) * 10;
                rightSpeed = rightVel.sqrMagnitude;
                leftLastPos = leftCurrentPos;
                rightLastPos = rightCurrentPos;
                yield return velocityWait;
            }
            else yield return null;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if ((groundMask & (1 << collision.gameObject.layer)) == 0) return;
        colliding = true;
        //collisionNormal = collision.GetContact(0).normal;
    }
    void OnCollisionStay(Collision collision)
    {
        if ((groundMask & (1 << collision.gameObject.layer)) == 0) return;
        if (!colliding) colliding = true;
        //collisionNormal = collision.GetContact(0).normal;
    }
    void OnCollisionExit(Collision collision)
    {
        if ((groundMask & (1 << collision.gameObject.layer)) == 0) return;
        colliding = false;
    }

    IEnumerator ReloadCanvas(float duration, bool wait = false)
    {
        if (wait) yield return oneSecond;
        float elapsedTime = 0;
        float alpha = 0;
        while (alpha <= 0.99f)
        {
            alpha = resetCanvas.alpha = elapsedTime / duration;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        resetCanvas.alpha = 1;
    }

    IEnumerator Reload(float duration)
    {
        initialized = false;
        float elapsedTime = 0;
        float alpha = 0;
        while (alpha <= 0.99f)
        {
            alpha = canvasGroup.alpha = elapsedTime / duration;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        alpha = canvasGroup.alpha = 1;
        rb.velocity = rb.angularVelocity = Vector3.zero;
        rb.MovePosition(initialPos);
        rb.MoveRotation(initialRot);
        minutes = passedRings = 0;
        for (int i = 0; i < 16; i++) rings[i].Reset();
        seconds = 0;
        timeText.SetText("Time: 0:00");
        ringText.SetText("Rings: 0/16");
        resetCanvas.alpha = 0;
        elapsedTime = 0;
        while (alpha >= 0.01f)
        {
            alpha = canvasGroup.alpha = 1 - elapsedTime / duration;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 0;
        initialized = true;
    }

    public static void PassedRing()
    {
        passedRings ++;
        ringText.SetText("Rings: " + passedRings + "/16");
    }
}
