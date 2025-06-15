using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class Skier : MonoBehaviour
{
    [Header("Skier")]
    [SerializeField] Transform skiParent;
    [SerializeField] Transform camOffset;
    [SerializeField] Transform cam;
    [SerializeField] int alignStrength;
    float skiSpeed;
    [SerializeField] float skierDirectionWeight;
    [SerializeField] float minDrag;
    [SerializeField] int maxDrag;
    [SerializeField] float perpDeceleration;
    [SerializeField] LayerMask groundMask;
    public static int attachedSkis;
    [SerializeField] SkiController rightController;
    [SerializeField] SkiController leftController;

    [Header("Movement")]
    [SerializeField] float normalMoveForce;
    [SerializeField] int xMoveForce;
    [SerializeField] int zMoveForce;
    [SerializeField] int airMoveForce;
    [SerializeField] InputActionProperty moveActionProperty;
    InputAction moveAction;
    [SerializeField] float normalTurnForce;
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

    void Awake()
    {
        myT = transform;
        rb = GetComponent<Rigidbody>();
        moveAction = moveActionProperty.action;
        turnAction = turnActionProperty.action;
        crouchAction = crouchActionProperty.action;
        jumpAction = jumpActionProperty.action;
        leftFlipAction = leftFlipActionProperty.action;
        rightFlipAction = rightFlipActionProperty.action;
        resetAction = resetActionProperty.action;
        toggleController = toggleControllerProperty.action;
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
        cam.localPosition = Vector3.zero;
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
            if (leftFlipAction.IsPressed() && attachedSkis > 0 && !colliding) leftFlip = true;
            if (rightFlipAction.IsPressed() && attachedSkis > 0 && !colliding) rightFlip = true;
        }
        if (leftFlipAction.WasReleasedThisFrame()) leftFlip = false;
        if (rightFlipAction.WasReleasedThisFrame()) rightFlip = false;

        if (resetAction.WasPressedThisFrame()) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        if (toggleController.WasPressedThisFrame())
        {
            rightController.SwitchController();
            leftController.SwitchController();
        }

        //Crouch mechanics
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

        //No ski turning
        if (attachedSkis == 0)
        {
            Quaternion targetRotation = myT.rotation * Quaternion.Euler(new Vector3(0, turnAction.ReadValue<Vector2>().x, 0));
            myT.rotation = Quaternion.Slerp(myT.rotation, targetRotation, Mathf.Clamp01(normalTurnForce * Time.deltaTime));
        }
    }

    void LateUpdate()
    {
        //Skis follow camera rotation
        skiParent.rotation = cam.rotation;
        skiParent.localRotation = Quaternion.Euler(0, skiParent.localEulerAngles.y, 0);
    }

    void FixedUpdate()
    {
        //Movement input
        Vector2 turnInput = turnAction.ReadValue<Vector2>();
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        Vector3 currentUp = myT.up;

        //No skis
        if (attachedSkis == 0)
        {
            rb.AddForce(skiParent.TransformDirection(new Vector3(moveInput.x, 0, moveInput.y) * normalMoveForce), ForceMode.VelocityChange);
            if (Physics.Raycast(myT.position + currentUp * 1.7f, -currentUp, 1.8f, groundMask))
            {
                isGrounded = true;
                if (jump)
                {
                    rb.AddForce(skiParent.up * normalJumpForce, ForceMode.VelocityChange);
                    jump = false;
                }
            }
            else
            {
                if (!colliding) isGrounded = false;
                if (jump) jump = false;
            }
            if (rightFlip) rightFlip = false;
            if (leftFlip) leftFlip = false;
            return;
        }

        //Grounded raycast
        if (Physics.Raycast(myT.position + currentUp * 1.7f, -currentUp, out RaycastHit hit, 1.9f, groundMask))
        {
            //Skier stabilization
            Vector3 groundUp = hit.normal;
            Vector3 rotationAxis = Vector3.Cross(currentUp, groundUp);
            rb.AddTorque(rotationAxis.normalized * Vector3.Angle(currentUp, groundUp) * alignStrength);

            //Velocity control based on direction
            Vector3 downhill = Vector3.ProjectOnPlane(Vector3.down, groundUp).normalized;
            Vector3 skierDirection = Vector3.ProjectOnPlane(skiParent.forward, groundUp).normalized;
            float alignment = Vector3.Dot(downhill, skierDirection);
            rb.AddForce(Vector3.Slerp(downhill, skierDirection, Mathf.Clamp01(Mathf.Clamp01(alignment) * skierDirectionWeight)) * skiSpeed, ForceMode.Acceleration);
            float targetDrag = Mathf.Lerp(minDrag, maxDrag, 1 - Mathf.Clamp01(Mathf.Abs(alignment)));
            rb.drag = Mathf.Lerp(rb.drag, targetDrag, perpDeceleration);

            //Is on ground
            if (!isGrounded)
            {
                isGrounded = true;
                if (rightHaptics) rightDevice.SendHapticImpulse(0, 1, 0.25f);
                if (leftHaptics) leftDevice.SendHapticImpulse(0, 1, 0.25f);
            }
            rb.AddTorque(skiParent.TransformDirection(new Vector3(-turnInput.y, turnInput.x, 0)) * (groundTurnForce + crouchSpeedIncrease), ForceMode.Acceleration);
            rb.AddForce(skiParent.TransformDirection(new Vector3(moveInput.x * (xMoveForce + crouchSpeedIncrease), 0, moveInput.y * (zMoveForce + crouchSpeedIncrease))), ForceMode.Acceleration);
            if (jump)
            {
                rb.AddForce(skiParent.up * jumpForce, ForceMode.VelocityChange);
                jump = false;
            }
            if (rightFlip) rightFlip = false;
            if (leftFlip) leftFlip = false;
        }
        else if (colliding)
        {
            //Is tilted on ground
            rb.AddTorque(skiParent.TransformDirection(new Vector3(-turnInput.y, turnInput.x, 0)) * (groundTurnForce + crouchSpeedIncrease), ForceMode.Acceleration);
            rb.AddForce(skiParent.TransformDirection(new Vector3(moveInput.x * (xMoveForce + crouchSpeedIncrease), 0, moveInput.y * (zMoveForce + crouchSpeedIncrease))), ForceMode.Acceleration);
            if (jump) jump = false;
            if (rightFlip) rightFlip = false;
            if (leftFlip) leftFlip = false;
        }
        else
        {
            //Is in air
            if (isGrounded) isGrounded = false;
            rb.drag = minDrag;
            rb.AddTorque(skiParent.TransformDirection(new Vector3(-turnInput.y, turnInput.x, 0)) * airTurnForce, ForceMode.Acceleration);
            rb.AddForce(skiParent.TransformDirection(new Vector3(moveInput.x, 0, moveInput.y) * airMoveForce), ForceMode.Acceleration);
            if (jump) jump = false;
            if (leftFlip) rb.AddTorque(skiParent.TransformDirection(Vector3.forward) * flipForce, ForceMode.Acceleration);
            if (rightFlip) rb.AddTorque(skiParent.TransformDirection(Vector3.back) * flipForce, ForceMode.Acceleration);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer != 9) return;
        colliding = true;
        //collisionNormal = collision.GetContact(0).normal;
        if (!isGrounded)
        {
            if (rightHaptics) rightDevice.SendHapticImpulse(0, 1, 0.25f);
            if (leftHaptics) leftDevice.SendHapticImpulse(0, 1, 0.25f);
        }
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
    }
}
