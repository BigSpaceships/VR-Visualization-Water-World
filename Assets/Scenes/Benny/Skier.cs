using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Skier : MonoBehaviour
{
    [Header("Skier")]
    [SerializeField] Transform skiParent;
    [SerializeField] Transform cam;
    [SerializeField] int alignStrength;
    [SerializeField] int maxAlignAngle;
    [SerializeField] int baseSkiSpeed;
    [SerializeField] float crouchSpeedIncrease;
    float skiSpeed;
    [SerializeField] float skierDirectionWeight;
    [SerializeField] float minDrag;
    [SerializeField] int maxDrag;
    [SerializeField] LayerMask groundMask;

    [Header("Movement")]
    [SerializeField] int xMoveForce;
    [SerializeField] int zMoveForce;
    [SerializeField] int airMoveForce;
    float speedIncrease;
    float initialCamHeight;
    [SerializeField] InputActionProperty moveActionProperty;
    InputAction moveAction;
    [SerializeField] int groundTurnForce;
    [SerializeField] int airTurnForce;
    [SerializeField] InputActionProperty turnActionProperty;
    InputAction turnAction;

    [Header("Jump")]
    [SerializeField] int jumpForce;
    [SerializeField] int flipForce;
    [SerializeField] InputActionProperty jumpActionProperty;
    [SerializeField] InputActionProperty jump2ActionProperty;
    InputAction jumpAction;
    InputAction jump2Action;
    bool isGrounded = true;
    bool jump;
    [SerializeField] InputActionProperty leftFlipActionProperty;
    [SerializeField] InputActionProperty rightFlipActionProperty;
    InputAction leftFlipAction;
    InputAction rightFlipAction;
    bool leftFlip;
    bool rightFlip;
    [SerializeField] InputActionProperty resetActionProperty;
    InputAction resetAction;
    [SerializeField] GameObject devSim;
    Rigidbody rb;
    Transform myT;

    void Awake()
    {
        myT = transform;
        rb = GetComponent<Rigidbody>();
        moveAction = moveActionProperty.action;
        turnAction = turnActionProperty.action;
        jumpAction = jumpActionProperty.action;
        jump2Action = jump2ActionProperty.action;
        leftFlipAction = leftFlipActionProperty.action;
        rightFlipAction = rightFlipActionProperty.action;
        resetAction = resetActionProperty.action;
        initialCamHeight = cam.localPosition.y;
        #if UNITY_EDITOR
        Instantiate(devSim);
        #endif
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
        //Fix camera
        Vector3 pos = cam.localPosition;
        cam.localPosition = new Vector3(0, pos.y, 0);
    }

    void Update()
    {
        //Ski faster when crouched
        speedIncrease = initialCamHeight - cam.localPosition.y * crouchSpeedIncrease;
        skiSpeed = baseSkiSpeed  + speedIncrease;
        
        if (skiSpeed < 0) skiSpeed = 0;

        //Input detection
        if (isGrounded)
        {
            if (jumpAction.WasPressedThisFrame() ||jump2Action.WasPressedThisFrame()) jump = true;
        }
        else
        {
            if (leftFlipAction.IsPressed()) leftFlip = true;
            else if (leftFlipAction.WasReleasedThisFrame()) leftFlip = false;
            if (rightFlipAction.IsPressed()) rightFlip = true;
            else if (rightFlipAction.WasReleasedThisFrame()) rightFlip = false;
        }

        //Reset
        if (resetAction.WasPressedThisFrame()) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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


        //Grounded raycast
        Vector3 currentUp = myT.up;
        if (Physics.Raycast(myT.position, -currentUp, out RaycastHit hit, 1.5f, groundMask))
        {
            //Skier stabilization
            Vector3 groundUp = hit.normal;
            Vector3 rotationAxis = Vector3.Cross(currentUp, groundUp);
            float angle = Vector3.Angle(currentUp, groundUp);
            if (angle < maxAlignAngle) rb.AddTorque(rotationAxis.normalized * angle * alignStrength);

            //Velocity control based on direction
            Vector3 downhill = Vector3.ProjectOnPlane(Vector3.down, groundUp).normalized;
            Vector3 skierDirection = Vector3.ProjectOnPlane(skiParent.forward, groundUp).normalized;
            float alignment = Vector3.Dot(downhill, skierDirection);
            rb.AddForce(Vector3.Slerp(downhill, skierDirection, Mathf.Clamp01(Mathf.Clamp01(alignment) * skierDirectionWeight)) * skiSpeed, ForceMode.Acceleration);
            rb.drag = Mathf.Lerp(minDrag, maxDrag, 1 - Mathf.Clamp01(Mathf.Abs(alignment)));

            //Is on ground
            isGrounded = true;
            turnInput.y = 0;
            rb.AddTorque(skiParent.TransformDirection(new Vector3(-turnInput.y, turnInput.x, 0)) * groundTurnForce, ForceMode.Acceleration);
            rb.AddForce(skiParent.TransformDirection(new Vector3(moveInput.x * (xMoveForce + speedIncrease), 0, moveInput.y * (zMoveForce + speedIncrease))), ForceMode.Acceleration);
            if (jump)
            {
                rb.AddForce(skiParent.up * jumpForce, ForceMode.VelocityChange);
                jump = false;
            }
            if (rightFlip) rightFlip = false;
            if (leftFlip) leftFlip = false;
        }
        else
        {
            //Is in air
            isGrounded = false;
            rb.AddTorque(skiParent.TransformDirection(new Vector3(-turnInput.y, turnInput.x, 0)) * airTurnForce, ForceMode.Acceleration);
            rb.AddForce(skiParent.TransformDirection(new Vector3(moveInput.x, 0, moveInput.y) * airMoveForce), ForceMode.Acceleration);
            if (jump) jump = false;
            if (leftFlip) rb.AddTorque(skiParent.TransformDirection(Vector3.forward) * flipForce, ForceMode.Acceleration);
            if (rightFlip) rb.AddTorque(skiParent.TransformDirection(Vector3.back) * flipForce, ForceMode.Acceleration);
        }
    }
}
