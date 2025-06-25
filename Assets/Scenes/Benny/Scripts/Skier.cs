using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class Skier : MonoBehaviour {
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
    bool leftHaptics, rightHaptics;

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
    public static bool isGrounded = true;
    bool colliding;
    //Vector3 collisionNormal;
    bool jump;
    bool isJumping;
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
    public static Rigidbody rb;
    public static Transform myT;
    UnityEngine.XR.InputDevice rightDevice;
    UnityEngine.XR.InputDevice leftDevice;

    [Header("Paraglider")]
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
    public static bool paragliding;
    public static Transform parachute;
    Vector3 leftVel, rightVel, leftLastPos, rightLastPos;
    float leftSpeed, rightSpeed;
    WaitForSeconds velocityWait = new WaitForSeconds(0.1f);
    public static List<Ring> rings = new List<Ring>(16);

    [Header("UI/Audio")]
    [SerializeField] AudioClip solidWalk;
    [SerializeField] AudioClip snowWalk;
    [SerializeField] AudioClip skiing;
    [SerializeField] AudioClip groundSkiing;
    [SerializeField] AudioClip skiWind;
    [SerializeField] AudioClip bgSkiWind;
    [SerializeField] AudioClip paraglideWind;
    [SerializeField] AudioClip bgParaglideWind;
    [SerializeField] AudioClip solidLand;
    [SerializeField] AudioClip snowLand;
    [SerializeField] AudioClip updraftSound;
    [SerializeField] AudioClip switchController;
    [SerializeField] AudioClip reset;
    AudioSource musicSource;
    AudioSource walkSource;
    AudioSource windSource;
    AudioSource bgWindSource;
    public static AudioSource effectSource;

    public static CanvasGroup canvasGroup;
    CanvasGroup resetCanvas;
    TMP_Text timeText, altText, speedText;
    public static TMP_Text ringText;
    public static int passedRings;
    int minutes;
    float seconds;
    Vector3 initialPos = new Vector3(-122.5f, 577.51f, -100.7f);
    Quaternion initialRot = Quaternion.Euler(new Vector3(0, 15, 0));
    Coroutine reload;
    WaitForSeconds oneSecond = new WaitForSeconds(1);
    public static bool initialized;

    private float playerHeightAdj = -1.3f;
    private CharacterController _cc;
    private Vector3 cc_velocity;
    private Vector3 cc_angularVelocity;
    private float cc_mass = 80f;
    private float cc_drag = 3f;
    private float cc_angularDrag = 3f;
    private bool cc_useGravity = true;
    private float cc_gravityScale = 1f;    // 重力倍数（1 = Physics.gravity）


    void Awake() {
        myT = transform;
        _cc = GetComponent<CharacterController>();
        rb = Pole.skier = transform.parent.gameObject.GetComponent<Rigidbody>();
        Skis.leftController = Pole.leftController = leftController;
        Skis.rightController = Pole.rightController = rightController;
        Skis.leftSkiController = Pole.leftSkiController = Parachute.leftSkiController = Rope.leftController = leftSkiController = leftController.GetComponent<SkiController>();
        Skis.rightSkiController = Pole.rightSkiController = Parachute.rightSkiController = Rope.rightController = rightSkiController = rightController.GetComponent<SkiController>();
        Transform leftRay = leftController.GetChild(2);
        Parachute.leftRay = leftRay.GetComponent<XRRayInteractor>();
        Parachute.leftRayVisual = leftRay.GetComponent<XRInteractorLineVisual>();
        Transform rightRay = rightController.GetChild(2);
        Parachute.rightRay = rightRay.GetComponent<XRRayInteractor>();
        Parachute.rightRayVisual = rightRay.GetComponent<XRInteractorLineVisual>();
        if (hands) {
            Rope.leftTarget = leftHandRopeTarget;
            Rope.rightTarget = rightHandRopeTarget;
            Pole.leftAttach = new Vector3(-0.035f, -1.28f, -0.03f);
            Pole.rightAttach = new Vector3(0.035f, -1.28f, -0.03f);
            leftSkiController.SwitchController(true);
            rightSkiController.SwitchController(true);
        } else {
            Rope.leftTarget = leftControllerRopeTarget;
            Rope.rightTarget = rightControllerRopeTarget;
            Pole.rightAttach = Pole.leftAttach = new Vector3(0, -1.3f, 0.05f);
            leftSkiController.SwitchController(false);
            rightSkiController.SwitchController(false);
        }
        interactableParent = Parachute.selectedParent = Skis.skiParent = Pole.skiParent = myT.Find("InteractableParent");
        moveAction = moveActionProperty.action;
        turnAction = turnActionProperty.action;
        crouchAction = crouchActionProperty.action;
        jumpAction = jumpActionProperty.action;
        leftFlipAction = leftFlipActionProperty.action;
        rightFlipAction = rightFlipActionProperty.action;
        resetAction = resetActionProperty.action;
        toggleController = toggleControllerProperty.action;

        //UI/Audio Setup
        resetCanvas = cam.GetChild(0).GetComponent<CanvasGroup>();
        resetCanvas.alpha = 0;
        Transform statCanvas = cam.GetChild(1);
        timeText = statCanvas.GetChild(0).GetComponent<TMP_Text>();
        altText = statCanvas.GetChild(1).GetComponent<TMP_Text>();
        speedText = statCanvas.GetChild(2).GetComponent<TMP_Text>();
        ringText = statCanvas.GetChild(3).GetComponent<TMP_Text>();
        ringText.enabled = false;
        AudioSource[] audioSources = GetComponents<AudioSource>();
        musicSource = audioSources[0];
        walkSource = audioSources[1];
        windSource = audioSources[2];
        bgWindSource = audioSources[3];
        effectSource = Skis.effectSource = Pole.effectSource = Parachute.effectSource = Ring.effectSource = audioSources[4];
        altText.SetText("Altitude: " + (initialPos.y * 3.281f).ToString("0") + "ft");
    }

    /// <summary>
    /// 模拟 Rigidbody.AddForce
    /// </summary>
    public void AddForce(Vector3 force, ForceMode mode = ForceMode.Force) {
        switch (mode) {
            case ForceMode.Force:
                // F = m·a  => a = F/m
                cc_velocity += (force / cc_mass) * Time.deltaTime;
                break;

            case ForceMode.Acceleration:
                // 直接当作加速度
                cc_velocity += force * Time.deltaTime;
                break;

            case ForceMode.Impulse:
                // 瞬时速度增量 a = F·Δt/m, 但 Δt 在 RigidbodyImpulse 中为 1 帧，这里直接用 force/m
                cc_velocity += force / cc_mass;
                break;

            case ForceMode.VelocityChange:
                // 瞬时速度增量
                cc_velocity += force;
                break;
        }
    }

    /// <summary>
    /// 模拟 Rigidbody.AddTorque
    /// </summary>
    public void AddTorque(Vector3 torque, ForceMode mode = ForceMode.Force) {
        switch (mode) {
            case ForceMode.Force:
                // 简化处理：把质量当做转动惯量
                cc_angularVelocity += (torque / cc_mass) * Time.deltaTime;
                break;

            case ForceMode.Acceleration:
                cc_angularVelocity += torque * Time.deltaTime;
                break;

            case ForceMode.Impulse:
                cc_angularVelocity += torque / cc_mass;
                break;

            case ForceMode.VelocityChange:
                cc_angularVelocity += torque;
                break;
        }
    }

    private bool active = false;

    public void activeSki() {
        camOffset.localPosition = new Vector3(0, 1.7f + playerHeightAdj, 0);
        GamePublicV2.instance.setController(ControllerName.Ski);
        GamePublicV2.instance.setMoveMode(MoveMode.Ski);
        rightDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        if (rightDevice.isValid && rightDevice.TryGetHapticCapabilities(out HapticCapabilities capabilities) && capabilities.supportsImpulse) rightHaptics = true;
        else rightHaptics = false;
        leftDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        if (leftDevice.isValid && leftDevice.TryGetHapticCapabilities(out capabilities) && capabilities.supportsImpulse) leftHaptics = true;
        else leftHaptics = false;
        Pole.leftDevice = leftDevice;
        Pole.rightDevice = rightDevice;
        Pole.leftHaptics = leftHaptics;
        Pole.rightHaptics = rightHaptics;
        StartCoroutine(CalculateVelocity());
        StartCoroutine(Reload(0.5f));
        myT.SetPositionAndRotation(initialPos, initialRot);
        turnAction.Enable();
        active = true;
        initialized = true;
    }


    void OnEnable() {
        if (!active) return;
    }


    private bool wasGrounded = false;

    void Update() {
        if (!active) return;
        //if (SceneManager.sceneCount > 1) for (int i = 0; i < SceneManager.loadedSceneCount; i++) if (SceneManager.GetSceneAt(i).name != "Skiing") SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(i));
        if (!initialized) return;

        if (cc_useGravity) {
            if (!_cc.isGrounded)
                cc_velocity += Physics.gravity * cc_gravityScale * Time.deltaTime;
            else if (cc_velocity.y < 0f)
                cc_velocity.y = 0f;
        }

        // 2. 线性阻力 (drag)：
        //    与 Rigidbody 相同的衰减： v_new = v_old / (1 + drag * dt)
        cc_velocity = cc_velocity / (1f + cc_drag * Time.deltaTime);

        // 3. 角阻力 (angularDrag)
        cc_angularVelocity =cc_angularVelocity / (1f + cc_angularDrag * Time.deltaTime);

        // 4. 进行运动和旋转
        if (cc_velocity != Vector3.zero)
            _cc.Move(cc_velocity * Time.deltaTime);

        if (cc_angularVelocity != Vector3.zero)
            transform.Rotate(cc_angularVelocity * Time.deltaTime, Space.World);

        //collision control system
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        float maxDist = 1.9f + 0.1f;
        // 调试射线可视化（选用）
        Debug.DrawRay(origin, Vector3.down * maxDist, Color.red);
        // 只会检测 groundMask 里的 Layer
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, maxDist, groundMask)) {
            if (!wasGrounded) {
                // 刚落地
                HandleGroundEnter(hit.normal);
            } else {
                // 仍在地面
                HandleGroundStay(hit.normal);
            }
            wasGrounded = true;
        } else {
            // 刚离地
            if (wasGrounded)
                HandleGroundExit();
            wasGrounded = false;
        }

        //Input detection
        if (isGrounded) {
            if (jumpAction.WasPressedThisFrame()) {
                jump = true;
            }
        } else {
            if (leftFlipAction.IsPressed() && attachedSkis > 0 && !paragliding) leftFlip = true;
            if (rightFlipAction.IsPressed() && attachedSkis > 0 && !paragliding) rightFlip = true;
        }
        if (leftFlipAction.WasReleasedThisFrame()) leftFlip = false;
        if (rightFlipAction.WasReleasedThisFrame()) rightFlip = false;

        if (resetAction.WasPressedThisFrame()) StartCoroutine(Reload(0.5f));
        if (toggleController.WasPressedThisFrame()) {
            hands = !hands;
            if (hands) {
                Rope.leftTarget = leftHandRopeTarget;
                Rope.rightTarget = rightHandRopeTarget;
                Pole.rightAttach = new Vector3(0.035f, -1.28f, -0.03f);
                Pole.leftAttach = new Vector3(-0.035f, -1.28f, -0.03f);
                leftSkiController.SwitchController(true, paragliding);
                rightSkiController.SwitchController(true, paragliding);
            } else {
                Rope.leftTarget = leftControllerRopeTarget;
                Rope.rightTarget = rightControllerRopeTarget;
                Pole.rightAttach = Pole.leftAttach = new Vector3(0, -1.3f, 0.05f);
                leftSkiController.SwitchController(false);
                rightSkiController.SwitchController(false);
            }
            effectSource.PlayOneShot(switchController);
        }

        //Crouch mechanics
        float height = camOffset.localPosition.y;
        if (crouchAction.IsPressed() && (!paragliding || isGrounded)) {
            skiSpeed = baseSkiSpeed + crouchSpeedIncrease;
            if (height != crouchHeight + playerHeightAdj) height = Mathf.Lerp(height, crouchHeight + playerHeightAdj, Mathf.Clamp01(Time.deltaTime * crouchSpeed));
        } else {
            skiSpeed = baseSkiSpeed;
            if (height != 1.7f + playerHeightAdj) height = Mathf.Lerp(height, 1.7f + playerHeightAdj, Mathf.Clamp01(Time.deltaTime * crouchSpeed));
        }
        camOffset.localPosition = new Vector3(0, height, 0);

        Vector3 pos = myT.position;
        if (Mathf.Abs(pos.x) >= 1500 || Mathf.Abs(pos.z) >= 1500) {
            StartCoroutine(Reload(0.5f));
            return;
        }
        if (reload == null) {
            if (Mathf.Abs(pos.y) >= 1500) reload = StartCoroutine(ReloadCanvas(0.5f));
            else if (paragliding && passedRings == 16) reload = StartCoroutine(ReloadCanvas(0.5f, true));
            else {
                float posMag = new Vector2(pos.x, pos.z).sqrMagnitude;
                if (posMag >= 1400000) reload = StartCoroutine(ReloadCanvas(0.5f));
            }
        }

        //UI
        seconds += Time.deltaTime;
        if (Mathf.Round(seconds) >= 60) {
            seconds = 0;
            minutes++;
        }
        timeText.SetText("Time: " + minutes + ":" + seconds.ToString("00"));
        altText.SetText("Altitude: " + (myT.position.y * 3.281f).ToString("0") + "ft");
        speedText.SetText("Speed: " + (cc_velocity.magnitude * 3.281).ToString("0.0") + "ft/s");
    }

    void LateUpdate() {
        //Interactables follow camera rotation
        if (!active) return;
        //interactableParent.rotation = cam.rotation; //!!!
        interactableParent.localRotation = Quaternion.Euler(0, interactableParent.localEulerAngles.y, 0);
    }

    void FixedUpdate() {
        if (!active) return;
        if (!initialized) return;

        //Movement input
        Vector2 turnInput = turnAction.ReadValue<Vector2>();
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        Vector3 currentUp = myT.up;
        RaycastHit hit;

        //Paragliding mechanics
        if (paragliding) {
            //Parachute adjustment
            Vector3 leftPos = interactableParent.InverseTransformPoint(leftController.position);
            Vector3 rightPos = interactableParent.InverseTransformPoint(rightController.position);
            float roll = Vector3.SignedAngle(Vector3.right, Vector3.ProjectOnPlane(rightPos - leftPos, Vector3.forward).normalized, Vector3.forward);
            roll = Mathf.Clamp(roll / 2, -45, 45);
            if (Mathf.Abs(roll) < 5) roll = 0;
            float t = Mathf.Clamp01(parachuteAdjustmentSpeed * Time.fixedDeltaTime);
            parachute.GetLocalPositionAndRotation(out Vector3 pos, out Quaternion qRot);
            Vector3 rot = qRot.eulerAngles;
            Vector3 targetPos = (leftPos + rightPos) / 2 + new Vector3(0, 0.1f + cc_velocity.y * parachuteVelocityOffset, 0.2f);
            targetPos.y = Mathf.Clamp(targetPos.y, 1.4f, 2.4f);
            parachute.SetLocalPositionAndRotation(Vector3.Lerp(pos, targetPos, t), Quaternion.Slerp(qRot, Quaternion.Euler(roll, rot.y, 0), t));

            //Is grounded
            if (Physics.Raycast(myT.position, -currentUp, out hit, (1.8f - playerHeightAdj), groundMask)) {
                SwitchClip(bgWindSource, bgSkiWind);
                //Regular paragliding
                if (attachedSkis == 0) {
                    if (windSource.isPlaying) windSource.Pause();
                    if (hit.transform.gameObject.layer == 9) {
                        if (!isGrounded) {
                            isGrounded = cc_useGravity = rb.freezeRotation = true;
                            cc_drag = 3;
                            effectSource.PlayOneShot(snowLand);
                            if (leftHaptics) leftDevice.SendHapticImpulse(0, 0.75f, 0.2f);
                            if (rightHaptics) rightDevice.SendHapticImpulse(0, 0.75f, 0.2f);
                        }
                        SwitchWalkSource(snowWalk);
                    } else {
                        if (!isGrounded) {
                            isGrounded = cc_useGravity = rb.freezeRotation = true;
                            cc_drag = 3;
                            effectSource.PlayOneShot(solidLand);
                            if (leftHaptics) leftDevice.SendHapticImpulse(0, 0.75f, 0.2f);
                            if (rightHaptics) rightDevice.SendHapticImpulse(0, 0.75f, 0.2f);
                        }
                        SwitchWalkSource(solidWalk);
                    }
                    Quaternion myRot = myT.rotation;
                    myT.rotation = Quaternion.Slerp(myRot, myRot * Quaternion.Euler(0, turnInput.x * normalTurnForce, 0), Time.fixedDeltaTime);
                    AddForce(interactableParent.TransformDirection(moveInput.x, 0, moveInput.y).normalized * normalMoveForce, ForceMode.VelocityChange);
                }
                //Parachute skiing (speedrunning)
                else {
                    int layer = hit.transform.gameObject.layer;
                    if (layer == 9) {
                        if (!isGrounded) {
                            isGrounded = true;
                            cc_useGravity = rb.freezeRotation = false;
                            if (isJumping) {
                                effectSource.PlayOneShot(snowLand);
                                if (leftHaptics) leftDevice.SendHapticImpulse(0, 0.75f, 0.2f);
                                if (rightHaptics) rightDevice.SendHapticImpulse(0, 0.75f, 0.2f);
                                isJumping = false;
                            }
                        }
                        if (!musicSource.isPlaying) musicSource.PlayDelayed(1);
                        SwitchWalkSource(skiing);
                        SwitchClip(windSource, skiWind);
                    } else {
                        if (!isGrounded) {
                            isGrounded = true;
                            cc_useGravity = rb.freezeRotation = false;
                            if (isJumping) {
                                effectSource.PlayOneShot(solidLand);
                                if (leftHaptics) leftDevice.SendHapticImpulse(0, 0.75f, 0.2f);
                                if (rightHaptics) rightDevice.SendHapticImpulse(0, 0.75f, 0.2f);
                                isJumping = false;
                            }
                        }
                        SwitchWalkSource(groundSkiing);
                        if (windSource.isPlaying) windSource.Pause();
                    }

                    //Skier stabilization
                    AddForce(Vector3.down * skiGravity);
                    Vector3 groundUp = hit.normal;
                    Vector3 rotationAxis = Vector3.Cross(currentUp, groundUp);
                    AddTorque(rotationAxis.normalized * Vector3.Angle(currentUp, groundUp) * alignStrength);
                    if (layer != 14) {
                        //Velocity control based on direction
                        Vector3 downhill = Vector3.ProjectOnPlane(Vector3.down, groundUp).normalized;
                        Vector3 skierDirection = Vector3.ProjectOnPlane(interactableParent.forward, groundUp).normalized;
                        float alignment = Vector3.Dot(downhill, skierDirection);
                        AddForce(skierDirection * skiSpeed * Mathf.Clamp01(alignment + 1), ForceMode.Acceleration);
                        float targetDrag = Mathf.Lerp(minDrag, maxDrag, 1 - Mathf.Clamp01(Mathf.Abs(alignment) * 1.5f));
                        cc_drag = Mathf.Lerp(cc_drag, targetDrag, perpDeceleration);
                    } else cc_drag = 3;
                    AddForce(interactableParent.TransformDirection(moveInput.x * xMoveForce, 0, moveInput.y * zMoveForce), ForceMode.Acceleration);
                    AddTorque(interactableParent.TransformDirection(0, turnInput.x, 0) * groundTurnForce, ForceMode.Acceleration);
                }
                if (jump) {
                    if (rb.freezeRotation) rb.freezeRotation = false;
                    AddForce(interactableParent.up * paraglideJumpForce, ForceMode.VelocityChange);
                    StartCoroutine(Updraft(3000, interactableParent.TransformDirection(0, 1, -1).normalized));
                    jump = false;
                    isJumping = true;
                    if (!musicSource.isPlaying) musicSource.PlayDelayed(1);
                }
            }
            //Is tilted on ground
            else if (colliding) {
                SwitchClip(bgWindSource, bgSkiWind);
                if (attachedSkis > 0) {
                    if (cc_useGravity) {
                        cc_useGravity = rb.freezeRotation = false;
                        cc_drag = minDrag;
                    }
                    AddForce(Vector3.down * skiGravity);
                    AddForce(interactableParent.TransformDirection(moveInput.x * xMoveForce, 0, moveInput.y * zMoveForce), ForceMode.Acceleration);
                    AddTorque(interactableParent.TransformDirection(-turnInput.y, turnInput.x, 0).normalized * groundTurnForce, ForceMode.Acceleration);
                } else {
                    if (!cc_useGravity) {
                        cc_useGravity = rb.freezeRotation = true;
                        cc_drag = 3;
                    }
                }
                if (jump) jump = false;
            }
            //Is in air
            else {
                if (jump) jump = false;
                if (isGrounded) {
                    isGrounded = cc_useGravity = rb.freezeRotation = false;
                    cc_drag = 5;
                    if (walkSource.isPlaying) walkSource.Pause();
                    SwitchClip(windSource, paraglideWind);
                    SwitchClip(bgWindSource, bgParaglideWind);
                }

                //Gliding/steering
                Vector3 rotationAxis = Vector3.Cross(currentUp, Vector3.up);
                AddTorque(rotationAxis.normalized * Vector3.Angle(currentUp, Vector3.up) * alignStrength);
                float distance = Vector3.Distance(leftPos, rightPos);
                if (distance < 0.02f) distance = 0.02f;
                Vector3 scale = parachute.localScale;
                parachute.localScale = new Vector3(scale.x, scale.y, Mathf.Lerp(scale.z, Mathf.Clamp(distance * 5, 0.4f, 1.2f), t));
                AddForce(Vector3.down * paraglideGravity * Mathf.Clamp(Mathf.Pow(0.2f / distance, 3), 0.2f, 15));
                AddForce(interactableParent.forward * glideForce, ForceMode.Acceleration);
                AddForce(interactableParent.TransformDirection(moveInput.x + turnInput.x, 0, moveInput.y + turnInput.y).normalized * inputGlideForce, ForceMode.Acceleration);
                float startValue = rot.x;
                if (startValue > 180) startValue -= 360;
                AddTorque(-interactableParent.up * Mathf.Lerp(startValue, roll, t) * paraglideTurnForce, ForceMode.Acceleration);
                if (leftSpeed > 1 && rightSpeed > 1 && Vector3.Dot(leftVel.normalized, interactableParent.up) < -0.7f && Vector3.Dot(rightVel.normalized, interactableParent.up) < -0.7f) {
                    AddForce(interactableParent.up * (leftSpeed + rightSpeed) * 40, ForceMode.Acceleration);
                    effectSource.PlayOneShot(updraftSound, 0.3f);
                }
            }
            if (rightFlip) rightFlip = false;
            if (leftFlip) leftFlip = false;
            return;
        }

        //No skis
        if (attachedSkis == 0) {
            if (!rb.freezeRotation) {
                rb.freezeRotation = rb.useGravity = true;
                cc_drag = minDrag;
            }
            AddForce(interactableParent.TransformDirection(moveInput.x, 0, moveInput.y).normalized * normalMoveForce, ForceMode.VelocityChange);
            if (windSource.isPlaying) windSource.Pause();
            SwitchClip(bgWindSource, bgSkiWind);
            if (Physics.Raycast(myT.position, -currentUp, out hit, (1.75f - playerHeightAdj), groundMask) && colliding) {
                Quaternion rot = myT.rotation;
                myT.rotation = Quaternion.Slerp(rot, rot * Quaternion.Euler(0, turnInput.x * normalTurnForce, 0), Time.fixedDeltaTime);

                if (hit.transform.gameObject.layer == 9) {
                    if (!isGrounded) {
                        isGrounded = true;
                        if (cc_drag == minDrag) cc_drag = 3;
                        if (isJumping) {
                            effectSource.PlayOneShot(snowLand);
                            if (leftHaptics) leftDevice.SendHapticImpulse(0, 0.75f, 0.2f);
                            if (rightHaptics) rightDevice.SendHapticImpulse(0, 0.75f, 0.2f);
                            isJumping = false;
                        }
                    }
                    SwitchWalkSource(snowWalk);
                } else {
                    if (!isGrounded) {
                        isGrounded = true;
                        if (cc_drag == minDrag) cc_drag = 3;
                        if (isJumping) {
                            effectSource.PlayOneShot(solidLand);
                            if (leftHaptics) leftDevice.SendHapticImpulse(0, 0.75f, 0.2f);
                            if (rightHaptics) rightDevice.SendHapticImpulse(0, 0.75f, 0.2f);
                            isJumping = false;
                        }
                    }
                    SwitchWalkSource(solidWalk);
                }
                if (jump) {
                    AddForce(interactableParent.up * normalJumpForce, ForceMode.VelocityChange);
                    jump = false;
                    isJumping = true;
                }
            } else if (colliding && jump) {
                jump = false;
            } else {
                isGrounded = false;
                if (jump) jump = false;
                if (walkSource.isPlaying) walkSource.Pause();
            }
            if (rightFlip) rightFlip = false;
            if (leftFlip) leftFlip = false;
            return;
        }

        //Attached skis
        AddForce(Vector3.down * skiGravity);
        SwitchClip(bgWindSource, bgSkiWind);
        if (Physics.Raycast(myT.position, -currentUp, out hit, 1.9f, groundMask)) {
            int layer = hit.transform.gameObject.layer;
            if (layer == 9) {
                if (!isGrounded) {
                    isGrounded = true;
                    if (isJumping) {
                        effectSource.PlayOneShot(snowLand);
                        if (leftHaptics) leftDevice.SendHapticImpulse(0, 0.75f, 0.2f);
                        if (rightHaptics) rightDevice.SendHapticImpulse(0, 0.75f, 0.2f);
                        isJumping = false;
                    }
                }
                if (!musicSource.isPlaying) musicSource.PlayDelayed(1);
                SwitchWalkSource(skiing);
                SwitchClip(windSource, skiWind);
            } else {
                if (!isGrounded) {
                    isGrounded = true;
                    if (isJumping) {
                        effectSource.PlayOneShot(solidLand);
                        if (leftHaptics) leftDevice.SendHapticImpulse(0, 0.75f, 0.2f);
                        if (rightHaptics) rightDevice.SendHapticImpulse(0, 0.75f, 0.2f);
                        isJumping = false;
                    }
                }
                SwitchWalkSource(groundSkiing);
                if (windSource.isPlaying) windSource.Pause();
            }

            //Skier stabilization
            Vector3 groundUp = hit.normal;
            Vector3 rotationAxis = Vector3.Cross(currentUp, groundUp);
            AddTorque(rotationAxis.normalized * Vector3.Angle(currentUp, groundUp) * alignStrength);
            if (layer != 14) {
                //Velocity control based on direction
                Vector3 downhill = Vector3.ProjectOnPlane(Vector3.down, groundUp).normalized;
                Vector3 skierDirection = Vector3.ProjectOnPlane(interactableParent.forward, groundUp).normalized;
                float alignment = Vector3.Dot(downhill, skierDirection);
                AddForce(skierDirection * skiSpeed * Mathf.Clamp01(alignment + 1), ForceMode.Acceleration);
                float targetDrag = Mathf.Lerp(minDrag, maxDrag, 1 - Mathf.Clamp01(Mathf.Abs(alignment) * 1.5f));
                cc_drag = Mathf.Lerp(cc_drag, targetDrag, perpDeceleration);
            } else cc_drag = 3;
            AddTorque(interactableParent.TransformDirection(0, turnInput.x, 0).normalized * (groundTurnForce + crouchSpeedIncrease), ForceMode.Acceleration);
            AddForce(interactableParent.TransformDirection(moveInput.x * (xMoveForce + crouchSpeedIncrease), 0, moveInput.y * (zMoveForce + crouchSpeedIncrease)), ForceMode.Acceleration);
            if (jump) {
                AddForce(interactableParent.up * jumpForce, ForceMode.VelocityChange);
                effectSource.PlayOneShot(updraftSound, 0.3f);
                isJumping = true;
                jump = false;
            }
            if (rightFlip) rightFlip = false;
            if (leftFlip) leftFlip = false;
        } else if (colliding) {
            //Is tilted on ground
            AddTorque(interactableParent.TransformDirection(-turnInput.y, turnInput.x, 0).normalized * (groundTurnForce + crouchSpeedIncrease), ForceMode.Acceleration);
            AddForce(interactableParent.TransformDirection(moveInput.x * (xMoveForce + crouchSpeedIncrease), 0, moveInput.y * (zMoveForce + crouchSpeedIncrease)), ForceMode.Acceleration);
            if (jump) jump = false;
            if (rightFlip) rightFlip = false;
            if (leftFlip) leftFlip = false;
        } else {
            //Is in air
            if (isGrounded) {
                isGrounded = false;
                if (isJumping && walkSource.isPlaying) walkSource.Pause();
            }
            cc_drag = minDrag;
            AddTorque(interactableParent.TransformDirection(-turnInput.y, turnInput.x, 0).normalized * airTurnForce, ForceMode.Acceleration);
            AddForce(interactableParent.TransformDirection(moveInput.x, 0, moveInput.y).normalized * airMoveForce, ForceMode.Acceleration);
            if (jump) jump = false;
            if (leftFlip) AddTorque(interactableParent.TransformDirection(Vector3.forward) * flipForce, ForceMode.Acceleration);
            if (rightFlip) AddTorque(interactableParent.TransformDirection(Vector3.back) * flipForce, ForceMode.Acceleration);
        }
    }

    IEnumerator Updraft(float updraftForce, Vector3 direction) {
        yield return updraftWait;
        AddForce(direction * updraftForce, ForceMode.Acceleration);
        effectSource.PlayOneShot(updraftSound, 0.7f);
    }

    IEnumerator CalculateVelocity() {
        while (!initialized) yield return null;
        SwitchClip(bgWindSource, bgSkiWind);

        //Calculate controller velocity for paragliding
        while (true) {
            if (paragliding && !isGrounded && !colliding) {
                Vector3 leftCurrentPos = leftController.localPosition;
                leftVel = (leftCurrentPos - leftLastPos) * 10;
                leftSpeed = leftVel.sqrMagnitude;
                Vector3 rightCurrentPos = rightController.localPosition;
                rightVel = (rightCurrentPos - rightLastPos) * 10;
                rightSpeed = rightVel.sqrMagnitude;
                leftLastPos = leftCurrentPos;
                rightLastPos = rightCurrentPos;
                yield return velocityWait;
            } else yield return null;
        }
    }

    //can not invoked. Collision controlled by Character Controller
    void OnCollisionEnter(Collision collision) {
        if (!active) return;
        if ((groundMask & (1 << collision.gameObject.layer)) == 0) return;
        colliding = true;
        //collisionNormal = collision.GetContact(0).normal;
    }


    //can not invoked. Collision controlled by Character Controller
    void OnCollisionStay(Collision collision) {
        if ((groundMask & (1 << collision.gameObject.layer)) == 0) return;
        if (!colliding) colliding = true;
        //collisionNormal = collision.GetContact(0).normal;
    }

    //can not invoked. Collision controlled by Character Controller
    void OnCollisionExit(Collision collision) {
        if ((groundMask & (1 << collision.gameObject.layer)) == 0) return;
        colliding = false;
    }

    /// <summary>
    /// compatible with OnCollisionEnter
    /// </summary>
    public void HandleGroundEnter(Vector3 normal) {
        colliding = true;
        Debug.Log("Ground Enter，法线=" + normal);
    }

    /// <summary>
    /// compatible with OnCollisionStay()
    /// </summary>
    public void HandleGroundStay(Vector3 normal) {
        if (!active) return;
        if (!colliding) colliding = true;
    }

    /// <summary>
    /// compatible with OnCollisionExit()
    /// </summary>
    public void HandleGroundExit() {
        colliding = false;
        Debug.Log("Ground Exit");
    }

    IEnumerator ReloadCanvas(float duration, bool wait = false) {
        if (wait) yield return oneSecond;
        float elapsedTime = 0;
        float alpha = 0;
        while (alpha <= 0.99f) {
            alpha = resetCanvas.alpha = elapsedTime / duration;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        resetCanvas.alpha = 1;
    }

    IEnumerator Reload(float duration) {
        initialized = false;
        effectSource.PlayOneShot(reset);
        float elapsedTime = 0;
        float alpha = 0;
        while (alpha <= 0.99f) {
            alpha = canvasGroup.alpha = elapsedTime / duration;
            elapsedTime += Time.deltaTime;
            Debug.Log(elapsedTime);
            yield return null;
        }
        alpha = canvasGroup.alpha = 1;
        cc_velocity = Vector3.zero;
        cc_angularVelocity = Vector3.zero;
        myT.position = initialPos;
        myT.rotation = initialRot;

        minutes = passedRings = 0;
        foreach (Ring ring in rings) ring.Reset();
        seconds = 0;
        timeText.SetText("Time: 0:00");
        ringText.SetText("Rings: 0/16");
        resetCanvas.alpha = 0;
        reload = null;
        elapsedTime = 0;
        while (alpha >= 0.01f) {
            alpha = canvasGroup.alpha = 1 - elapsedTime / duration;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 0;
        initialized = true;
    }

    public static void PassedRing() {
        passedRings++;
        ringText.SetText("Rings: " + passedRings + "/16");
    }

    void SwitchClip(AudioSource source, AudioClip clip) {
        if (source.clip != clip) {
            source.clip = clip;
            source.Play();
            return;
        } else if (!source.isPlaying) source.UnPause();
    }

    void SwitchWalkSource(AudioClip clip) {
        if (walkSource.clip != clip) {
            walkSource.clip = clip;
            walkSource.Play();
        }
        if (cc_velocity.sqrMagnitude < 1 && cc_angularVelocity.sqrMagnitude < 1) {
            if (walkSource.isPlaying) walkSource.Pause();
        } else if (!walkSource.isPlaying) walkSource.UnPause();
    }
}
