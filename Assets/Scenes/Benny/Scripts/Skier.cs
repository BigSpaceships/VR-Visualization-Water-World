using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

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
    bool rightHaptics;
    bool leftHaptics;

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
    [SerializeField] float paraglideTurnForce;
    [SerializeField] float parachuteVelocityOffset;
    Vector3 leftVel, rightVel, leftLastPos, rightLastPos;
    float leftSpeed, rightSpeed;
    WaitForSeconds velocityWait = new WaitForSeconds(0.1f);

    void Awake()
    {
        myT = transform;
        rb = GetComponent<Rigidbody>();
        interactableParent = myT.GetChild(0);
        moveAction = moveActionProperty.action;
        turnAction = turnActionProperty.action;
        crouchAction = crouchActionProperty.action;
        jumpAction = jumpActionProperty.action;
        leftFlipAction = leftFlipActionProperty.action;
        rightFlipAction = rightFlipActionProperty.action;
        resetAction = resetActionProperty.action;
        toggleController = toggleControllerProperty.action;
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
            Pole.leftAttach = new Vector3(0, -1.3f, 0.05f);
            Pole.rightAttach = new Vector3(0, -1.3f, 0.05f);
            leftSkiController.SwitchController(false);
            rightSkiController.SwitchController(false);
        }

#if UNITY_EDITOR
            Instantiate(devSim);
#endif
    }

    void Start()
    {
        camOffset.localPosition = new Vector3(0, 1.7f, 0);
        rightDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        if (rightDevice.isValid && rightDevice.TryGetHapticCapabilities(out HapticCapabilities capabilities) && capabilities.supportsImpulse) rightHaptics = true;
        else rightHaptics = false;
        leftDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        if (leftDevice.isValid && leftDevice.TryGetHapticCapabilities(out capabilities) && capabilities.supportsImpulse) leftHaptics = true;
        else leftHaptics = false;
        StartCoroutine(CalculateVelocity());
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
        //Clamp camera position
        Vector3 pos = cam.localPosition;
        cam.localPosition = new Vector3(Mathf.Clamp(pos.x, -0.5f, 0.5f), Mathf.Clamp(pos.y, -0.5f, 0.5f), Mathf.Clamp(pos.z, -0.5f, 0.5f));
    }

    void Update()
    {
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

        if (resetAction.WasPressedThisFrame()) SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);;
        if (toggleController.WasPressedThisFrame())
        {
            hands = !hands;
            if (hands)
            {
                Rope.leftTarget = leftHandRopeTarget;
                Rope.rightTarget = rightHandRopeTarget;
                leftSkiController.SwitchController(true);
                rightSkiController.SwitchController(true);
                Pole.rightAttach = new Vector3(0.035f, -1.28f, -0.03f);
                Pole.leftAttach = new Vector3(-0.035f, -1.28f, -0.03f);
            }
            else
            {
                Rope.leftTarget = leftControllerRopeTarget;
                Rope.rightTarget = rightControllerRopeTarget;
                leftSkiController.SwitchController(false);
                rightSkiController.SwitchController(false);
                Pole.rightAttach = Pole.leftAttach = new Vector3(0, -1.3f, 0.05f);
            }
        }

        //Crouch mechanics
        if (paragliding) return;
        float height = camOffset.localPosition.y;
        if (crouchAction.IsPressed())
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
    }

    void LateUpdate()
    {
        //Skis follow camera rotation
        interactableParent.rotation = cam.rotation;
        interactableParent.localRotation = Quaternion.Euler(0, interactableParent.localEulerAngles.y, 0);
    }

    void FixedUpdate()
    {
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
            Vector3 targetPos = (leftPos + rightPos) / 2 + new Vector3(0, 0.05f + rb.velocity.y * parachuteVelocityOffset, 0.2f);
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
                        if (rightHaptics) rightDevice.SendHapticImpulse(0, 0.5f, 0.1f);
                        if (leftHaptics) leftDevice.SendHapticImpulse(0, 0.5f, 0.1f);
                    }
                    myT.Rotate(new Vector3(0, turnAction.ReadValue<Vector2>().x * normalTurnForce * Time.fixedDeltaTime, 0));
                    rb.AddForce(interactableParent.TransformDirection(new Vector3(moveInput.x, 0, moveInput.y) * normalMoveForce), ForceMode.VelocityChange);
                }
                //Parachute skiing (speedrunning)
                else
                {
                    if (!isGrounded)
                    {
                        isGrounded = true;
                        rb.useGravity = false;
                        if (rightHaptics) rightDevice.SendHapticImpulse(0, 0.5f, 0.1f);
                        if (leftHaptics) leftDevice.SendHapticImpulse(0, 0.5f, 0.1f);
                    }
                    rb.AddForce(Vector3.down * skiGravity);
                    if (hit.collider.gameObject.layer != 14)
                    {
                        //Skier stabilization
                        Vector3 groundUp = hit.normal;
                        Vector3 rotationAxis = Vector3.Cross(currentUp, groundUp);
                        rb.AddTorque(rotationAxis.normalized * Vector3.Angle(currentUp, groundUp) * alignStrength);
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
                    rb.AddTorque(interactableParent.TransformDirection(new Vector3(-turnInput.y, turnInput.x, 0)) * groundTurnForce, ForceMode.Acceleration);
                }
                if (jump)
                {
                    if (rb.freezeRotation) rb.freezeRotation = false;
                    rb.AddForce(interactableParent.up * paraglideJumpForce, ForceMode.VelocityChange);
                    StartCoroutine(Updraft(30, 70, interactableParent.TransformDirection(new Vector3(0, 1, -1)).normalized));
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
                        if (rightHaptics) rightDevice.SendHapticImpulse(0, 0.5f, 0.1f);
                        if (leftHaptics) leftDevice.SendHapticImpulse(0, 0.5f, 0.1f);
                    }
                    rb.AddForce(Vector3.down * skiGravity);
                    rb.AddForce(interactableParent.TransformDirection(new Vector3(moveInput.x * xMoveForce, 0, moveInput.y * zMoveForce)), ForceMode.Acceleration);
                    rb.AddTorque(interactableParent.TransformDirection(new Vector3(-turnInput.y, turnInput.x, 0)) * groundTurnForce, ForceMode.Acceleration);
                }
                else if (!isGrounded)
                {
                    isGrounded = rb.useGravity = true;
                    rb.freezeRotation = true;
                    rb.drag = 3;
                    if (rightHaptics) rightDevice.SendHapticImpulse(0, 0.5f, 0.1f);
                    if (leftHaptics) leftDevice.SendHapticImpulse(0, 0.5f, 0.1f);
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
                float distance = Vector3.Distance(leftPos, rightPos);
                if (distance < 0.02f) distance = 0.02f;
                Vector3 scale = parachute.localScale;
                parachute.localScale = new Vector3(scale.x, scale.y, Mathf.Lerp(scale.z, Mathf.Clamp(distance * 5, 0.5f, 1.2f), t));
                rb.AddForce(Vector3.down * paraglideGravity * Mathf.Clamp(0.2f / distance, 0.2f, 10));
                rb.AddForce(interactableParent.forward * glideForce, ForceMode.Acceleration);
                float startValue = rot.x;
                if (startValue > 180) startValue -= 360;
                rb.AddTorque(-interactableParent.up * Mathf.Lerp(startValue, roll, t) * paraglideTurnForce, ForceMode.Acceleration);
                if (leftSpeed > 0.5f && rightSpeed > 0.5f && Vector3.Dot(leftVel.normalized, interactableParent.up) < -0.7f && Vector3.Dot(rightVel.normalized, interactableParent.up) < -0.7f)
                {
                    StartCoroutine(Updraft(10, (leftSpeed + rightSpeed) * 5, interactableParent.up, false));
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
                rb.freezeRotation = true;
                rb.drag = minDrag;
            }
            myT.Rotate(new Vector3(0, turnAction.ReadValue<Vector2>().x * normalTurnForce * Time.fixedDeltaTime, 0));
            rb.AddForce(interactableParent.TransformDirection(new Vector3(moveInput.x, 0, moveInput.y) * normalMoveForce), ForceMode.VelocityChange);
            if (Physics.Raycast(myT.position + currentUp * 1.7f, -currentUp, 1.75f, groundMask) && colliding)
            {
                if (!isGrounded)
                {
                    isGrounded = true;
                    if (rightHaptics) rightDevice.SendHapticImpulse(0, 0.5f, 0.1f);
                    if (leftHaptics) leftDevice.SendHapticImpulse(0, 0.5f, 0.1f);
                    if (rb.drag == minDrag) rb.drag = 3;
                }
                if (jump)
                {
                    rb.AddForce(interactableParent.up * normalJumpForce, ForceMode.VelocityChange);
                    jump = false;
                }
            }
            else
            {
                if (colliding && !isGrounded)
                {
                    isGrounded = true;
                    if (rightHaptics) rightDevice.SendHapticImpulse(0, 0.5f, 0.1f);
                    if (leftHaptics) leftDevice.SendHapticImpulse(0, 0.5f, 0.1f);
                }
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
            if (!isGrounded)
            {
                isGrounded = true;
                if (rightHaptics) rightDevice.SendHapticImpulse(0, 0.5f, 0.1f);
                if (leftHaptics) leftDevice.SendHapticImpulse(0, 0.5f, 0.1f);
            }

            if (hit.collider.gameObject.layer != 14)
            {
                //Skier stabilization
                Vector3 groundUp = hit.normal;
                Vector3 rotationAxis = Vector3.Cross(currentUp, groundUp);
                rb.AddTorque(rotationAxis.normalized * Vector3.Angle(currentUp, groundUp) * alignStrength);
                //Velocity control based on direction
                Vector3 downhill = Vector3.ProjectOnPlane(Vector3.down, groundUp).normalized;
                Vector3 skierDirection = Vector3.ProjectOnPlane(interactableParent.forward, groundUp).normalized;
                float alignment = Vector3.Dot(downhill, skierDirection);
                rb.AddForce(skierDirection * skiSpeed * Mathf.Clamp01(alignment + 1), ForceMode.Acceleration);
                float targetDrag = Mathf.Lerp(minDrag, maxDrag, 1 - Mathf.Clamp01(Mathf.Abs(alignment) * 1.5f));
                rb.drag = Mathf.Lerp(rb.drag, targetDrag, perpDeceleration);
            }
            else rb.drag = 3;
            rb.AddTorque(interactableParent.TransformDirection(new Vector3(-turnInput.y, turnInput.x, 0)) * (groundTurnForce + crouchSpeedIncrease), ForceMode.Acceleration);
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
            if (!isGrounded)
            {
                isGrounded = true;
                if (rightHaptics) rightDevice.SendHapticImpulse(0, 0.5f, 0.1f);
                if (leftHaptics) leftDevice.SendHapticImpulse(0, 0.5f, 0.1f);
            }
            rb.AddTorque(interactableParent.TransformDirection(new Vector3(-turnInput.y, turnInput.x, 0)) * (groundTurnForce + crouchSpeedIncrease), ForceMode.Acceleration);
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
            rb.AddTorque(interactableParent.TransformDirection(new Vector3(-turnInput.y, turnInput.x, 0)) * airTurnForce, ForceMode.Acceleration);
            rb.AddForce(interactableParent.TransformDirection(new Vector3(moveInput.x, 0, moveInput.y) * airMoveForce), ForceMode.Acceleration);
            if (jump) jump = false;
            if (leftFlip) rb.AddTorque(interactableParent.TransformDirection(Vector3.forward) * flipForce, ForceMode.Acceleration);
            if (rightFlip) rb.AddTorque(interactableParent.TransformDirection(Vector3.back) * flipForce, ForceMode.Acceleration);
        }
    }

    IEnumerator Updraft(int strength, float updraftForce, Vector3 direction, bool wait = true)
    {
        if (wait) yield return updraftWait;
        int i = 0;
        while (i < strength)
        {
            if (!isGrounded && !colliding) rb.AddForce(direction * updraftForce, ForceMode.Acceleration);
            yield return null;
            i++;
        }
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
        if (!isGrounded)
        {
            if (rightHaptics) rightDevice.SendHapticImpulse(0, 0.5f, 0.1f);
            if (leftHaptics) leftDevice.SendHapticImpulse(0, 0.5f, 0.1f);
        }
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
}
