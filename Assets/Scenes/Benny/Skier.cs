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
    [SerializeField] InputActionProperty moveActionProperty;
    InputAction moveAction;
    [SerializeField] int turnForce;
    [SerializeField] InputActionProperty turnActionProperty;
    InputAction turnAction;
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

    [Header("Skis")]
    [SerializeField] Transform rightSki;
    [SerializeField] Transform leftSki;
    [SerializeField] int skiAlignStrength;
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
        #if UNITY_EDITOR
            Instantiate(devSim);
        #endif
    }

    /*void OnEnable()
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
    }*/

    void Update()
    {
        //Ski faster when crouched
        skiSpeed = baseSkiSpeed - cam.localPosition.y * crouchSpeedIncrease;
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

        //Ski stabilization
        RaycastHit hit;
        Vector3 origin = rightSki.position + new Vector3(0, 0.5f, 0.7f);
        if (Physics.Raycast(origin, -rightSki.up, out hit, 2, groundMask))
        {
            Vector3 groundUp = hit.normal;
            if (Vector3.Angle(rightSki.up, groundUp) < maxAlignAngle)
            {
                Quaternion targetRotation = Quaternion.FromToRotation(rightSki.up, groundUp) * rightSki.rotation;
                rightSki.rotation = Quaternion.Slerp(rightSki.rotation, targetRotation, Time.deltaTime * skiAlignStrength);
            }
        }
        else
        {
            rightSki.localRotation = Quaternion.Slerp(rightSki.localRotation, quaternion.identity, Time.deltaTime * skiAlignStrength);
        }
        origin = leftSki.position + new Vector3(0, 0.5f, 0.7f);
        if (Physics.Raycast(origin, -leftSki.up, out hit, 2, groundMask))
        {
            Vector3 groundUp = hit.normal;
            if (Vector3.Angle(leftSki.up, groundUp) < maxAlignAngle)
            {
                Quaternion targetRotation = Quaternion.FromToRotation(leftSki.up, groundUp) * leftSki.rotation;
                leftSki.rotation = Quaternion.Slerp(leftSki.rotation, targetRotation, Time.deltaTime * skiAlignStrength);
            }
        }
        else
        {
            leftSki.localRotation = Quaternion.Slerp(leftSki.localRotation, quaternion.identity, Time.deltaTime * skiAlignStrength);
        }
    }

    void FixedUpdate()
    {
        //Move/Turn
        Vector2 input = turnAction.ReadValue<Vector2>();
        rb.AddTorque(skiParent.TransformDirection(new Vector3(-input.y, input.x, 0)) * turnForce, ForceMode.Acceleration);
        input = moveAction.ReadValue<Vector2>();
        rb.AddForce(skiParent.TransformDirection(new Vector3(input.x * xMoveForce, 0, input.y * zMoveForce)), ForceMode.Acceleration);

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

            //Jump
            isGrounded = true;
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
            //Flip
            isGrounded = false;
            if (jump) jump = false;
            if (leftFlip) rb.AddTorque(skiParent.TransformDirection(Vector3.forward) * flipForce, ForceMode.Acceleration);
            if (rightFlip) rb.AddTorque(skiParent.TransformDirection(Vector3.back) * flipForce, ForceMode.Acceleration);
        }
    }
}
