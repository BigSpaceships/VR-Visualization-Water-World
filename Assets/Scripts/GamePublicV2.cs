﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;
using UnityEngine.XR.Interaction.Toolkit.UI;
public enum MoveMode {
    None,
    Ground,
    UnderWater,
    Ski
}

public enum ControllerName {
    None,
    Main,
    A2_UnderWater,
    Ski
}

public class GamePublicV2 : MonoBehaviour {
    public GameObject test;
    public static GamePublicV2 instance;

    public MoveMode moveMode = MoveMode.None;
    public bool cameraActive = false;

    public GameObject HUD_UnderWater;
    public GameObject HUD_Ski1;
    public GameObject HUD_Ski2;

    [SerializeField] public Material skyBoxUnderWater;
    [SerializeField] public Material skyBoxGround;
    [SerializeField] public Material skyBoxSki;

    private Rigidbody playerRb;
    private CharacterController charController;
    private GameObject seaScooter;
    private CapsuleCollider xrOriginCollider;


    private GameObject controllerA2Left;
    private GameObject controllerA2Right;
    private GameObject controllerMainLeft;
    private GameObject controllerMainRight;
    private GameObject controllerSkiLeft;
    private GameObject controllerSkiRight;

    private ActionBasedContinuousMoveProvider moveProvider;
    private XRInputModalityManager xrInputModalityManager;

    [HideInInspector] public GameObject XROrigin;
    [HideInInspector] public GameObject XROriginRig;
    [HideInInspector] public GameObject persistentXR;
    [HideInInspector] public GameObject light;

    private Vector3 storedPlayerPosition;
    private Quaternion storedPlayerRotation;


    private void Awake() {
        if (instance == null) {
            instance = this;
        }

        persistentXR = GameObject.Find("PersistentXR");
        if (persistentXR == null) {
            //Debug.LogError("can not find GameObject: PersistentXR");
            return;
        }

        light = persistentXR.transform.Find("Light").gameObject;
        XROrigin = persistentXR.transform.Find("XR Origin").gameObject;
        XROriginRig = persistentXR.transform.Find("XR Origin/XR Origin (XR Rig)").gameObject;
        xrInputModalityManager = XROriginRig.GetComponent<XRInputModalityManager>(); ;
        playerRb = XROrigin.GetComponent<Rigidbody>();
        charController = XROriginRig.GetComponent<CharacterController>();
        seaScooter = GameObject.Find("SeaScooter");
        xrOriginCollider = XROrigin.GetComponent<CapsuleCollider>();
        moveProvider = XROriginRig.GetComponentInChildren<ActionBasedContinuousMoveProvider>();

        //bind all controller
        controllerMainLeft = XROriginRig.transform.Find("Camera Offset/Left Controller").gameObject;
        controllerMainRight = XROriginRig.transform.Find("Camera Offset/Right Controller").gameObject;
        controllerA2Left = XROriginRig.transform.Find("Camera Offset/Left Controller A2").gameObject;
        controllerA2Right = XROriginRig.transform.Find("Camera Offset/Right Controller A2").gameObject;
        controllerSkiLeft = XROriginRig.transform.Find("Camera Offset/Left Controller Ski").gameObject;
        controllerSkiRight = XROriginRig.transform.Find("Camera Offset/Right Controller Ski").gameObject;

        HUD_UnderWater = XROriginRig.transform.Find("Camera Offset/Main Camera/Canvas_HUD").gameObject;

        GameInit();
    }

    private void GameInit() {
        setMoveMode(MoveMode.Ground);
        setController(ControllerName.Main);
        StartCoroutine(movePlayerToStartPoint());
    }

    public IEnumerator movePlayerToStartPoint() {
        GameObject startPoint = persistentXR.transform.Find("PlayerStartPoint").GetChild(0).gameObject;
        if (startPoint.name == "DebugPoint") yield break;
        // 1. 瞬移玩家到起点
        XROriginRig.transform.SetPositionAndRotation(
            startPoint.transform.position,
            startPoint.transform.rotation
        );

        // (可选) 等一帧让所有系统刷新一下
        yield return null;

        Manager manager = GamePublicV2.instance.persistentXR.transform.Find("SkiManager").GetComponent<Manager>();
        float duration = 1f;

        //black screen
        float elapsedTime = 0;
        float alpha = 0;
        while (alpha <= 0.99f) {
            alpha = manager.resortCanvasGroup.alpha = elapsedTime / duration;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        alpha = manager.resortCanvasGroup.alpha = manager.resortCanvasGroup.alpha = 1;
        elapsedTime = 0;
        
        // 2. 等待自动地图加载系统把场景加载好
        //    假设场景名和 startPoint.name 一致
        yield return new WaitUntil(() => {
            var scene = SceneManager.GetSceneByName(startPoint.name);
            return scene.isLoaded;
        });

        // 3. 再次瞬移确保玩家不被地形/重力“拉下去”
        XROriginRig.transform.SetPositionAndRotation(
            startPoint.transform.position,
            startPoint.transform.rotation
        );
        
        //open eyes
        while (alpha >= 0.01f) {
            alpha = manager.resortCanvasGroup.alpha = 1 - elapsedTime / duration;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        manager.resortCanvasGroup.alpha = manager.resortCanvasGroup.alpha = 0;
    }

    public void savePlayerPosition() {
        if (checkVRActive()) {
            storedPlayerPosition = XROriginRig.transform.position;
            storedPlayerRotation = XROriginRig.transform.rotation;
        } else {
            storedPlayerPosition = XROrigin.transform.position;
            storedPlayerRotation = XROrigin.transform.rotation;
        }
    }

    public void loadPlayerPosition() {
        if (checkVRActive()) {
            XROriginRig.transform.position = storedPlayerPosition;
            XROriginRig.transform.rotation = storedPlayerRotation;
        } else {
            XROrigin.transform.position = storedPlayerPosition;
            XROrigin.transform.rotation = storedPlayerRotation;
        }
    }

    public bool checkVRActive() {
        bool active = XRSettings.isDeviceActive;
        var simHMD = InputSystem.GetDevice<XRSimulatedHMD>();
        if (simHMD != null) {
            active = true;
        }
        return active;
    }

    public ControllerName currentControllerName = ControllerName.None;

    public void setController(ControllerName name) {
        StartCoroutine(ResetAndSwitch(name));
    }

    public IEnumerator loadUnderWater() {
        Manager manager = GamePublicV2.instance.persistentXR.transform.Find("SkiManager").GetComponent<Manager>();
        float duration = 1f;

        //load new scene
        float elapsedTime = 0;
        float alpha = 0;
        while (alpha <= 0.99f) {
            alpha = manager.resortCanvasGroup.alpha = elapsedTime / duration;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        alpha = manager.resortCanvasGroup.alpha = manager.resortCanvasGroup.alpha = 1;
        elapsedTime = 0;
        
        GamePublicV2.instance.savePlayerPosition();
        setMoveMode(MoveMode.UnderWater);
        setController(ControllerName.A2_UnderWater);
        AreaLoaderController loader = GameObject.Find("AreaBorders").GetComponent<AreaLoaderController>();
        yield return StartCoroutine(loader.UnloadAllScenesCoroutine());
        AsyncOperation op = SceneManager.LoadSceneAsync("R_Area2 Under Water", LoadSceneMode.Additive);
        while (!op.isDone) yield return null;

        while (alpha >= 0.01f) {
            alpha = manager.resortCanvasGroup.alpha = 1 - elapsedTime / duration;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        manager.resortCanvasGroup.alpha = manager.resortCanvasGroup.alpha = 0;
    }

    public void unloadUnderWater() {
        StartCoroutine(unloadUnderWaterC());
    }
    
    public IEnumerator unloadUnderWaterC() {
        Manager manager = GamePublicV2.instance.persistentXR.transform.Find("SkiManager").GetComponent<Manager>();
        float duration = 1f;

        //load new scene
        float elapsedTime = 0;
        float alpha = 0;
        while (alpha <= 0.99f) {
            alpha = manager.resortCanvasGroup.alpha = elapsedTime / duration;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        alpha = manager.resortCanvasGroup.alpha = manager.resortCanvasGroup.alpha = 1;
        elapsedTime = 0;

        AreaLoaderController loader = GameObject.Find("AreaBorders").GetComponent<AreaLoaderController>();
        yield return StartCoroutine(loader.UnloadScene("R_Area2 Under Water"));
        yield return StartCoroutine(loader.RefreshCoroutine());
        GamePublicV2.instance.setMoveMode(MoveMode.Ground);
        GamePublicV2.instance.setController(ControllerName.Main);
        GamePublicV2.instance.loadPlayerPosition();

        while (alpha >= 0.01f) {
            alpha = manager.resortCanvasGroup.alpha = 1 - elapsedTime / duration;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        manager.resortCanvasGroup.alpha = manager.resortCanvasGroup.alpha = 0;
    }

    private IEnumerator ResetAndSwitch(ControllerName set) {
        DisableAllInteractors();
        yield return null;

        // 禁用所有控制器
        controllerMainLeft?.SetActive(false);
        controllerMainRight?.SetActive(false);
        controllerA2Left?.SetActive(false);
        controllerA2Right?.SetActive(false);
        controllerSkiLeft?.SetActive(false);
        controllerSkiRight?.SetActive(false);

        // 启用指定控制器组
        GameObject left = null, right = null;
        if (set == ControllerName.Main) {
            left = controllerMainLeft;
            right = controllerMainRight;
        } else if (set == ControllerName.A2_UnderWater) {
            left = controllerA2Left;
            right = controllerA2Right;
        } else if (set == ControllerName.Ski) {
            left = controllerSkiLeft;
            right = controllerSkiRight;
        }


        xrInputModalityManager.leftController = left;
        xrInputModalityManager.rightController = right;
        xrInputModalityManager.motionControllerModeStarted?.Invoke();

        left?.SetActive(true);
        right?.SetActive(true);

        currentControllerName = set;
    }

    private void DisableAllInteractors() {
        var interactors = FindObjectsByType<XRBaseInteractor>(FindObjectsSortMode.None);
        foreach (var interactor in interactors) {
            interactor.allowSelect = false;
            interactor.allowHover = false;
        }

        // 禁用可视化射线
        var visuals = FindObjectsByType<XRInteractorLineVisual>(FindObjectsSortMode.None);
        foreach (var v in visuals)
            v.enabled = false;
    }

    public void setMoveMode(MoveMode moveMode) {
        if (moveMode == this.moveMode) return;
        
        //release old mode
        if (this.moveMode == MoveMode.Ground) {

        } else if (this.moveMode == MoveMode.UnderWater) {

        }

        //initialize new mode
        HUD_UnderWater.SetActive(false);
        HUD_Ski1.SetActive(false);
        HUD_Ski2.SetActive(false);
        if (moveMode == MoveMode.Ground) {
            moveProvider.enabled = true;
            if (checkVRActive()) {
                playerRb.isKinematic = true;
                playerRb.useGravity = true;
                playerRb.constraints = RigidbodyConstraints.None;
                charController.enabled = true;
                seaScooter.SetActive(false);
                xrOriginCollider.enabled = false;
            } else {
                charController.enabled = false;
                playerRb.isKinematic = false;
                playerRb.useGravity = true;
                playerRb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
                seaScooter.SetActive(false);
                xrOriginCollider.enabled = true;
            }
            RenderSettings.skybox = skyBoxGround;
        } else if (moveMode == MoveMode.UnderWater) {
            HUD_UnderWater.SetActive(true);
            playerRb.isKinematic = true;   // 禁用刚体物理
            playerRb.useGravity = false;   // 水下无重力
            playerRb.constraints = RigidbodyConstraints.None; // 自由转动
            seaScooter.SetActive(true);
            moveProvider.enabled = false; //不使用系统提供的移动功能
            if (charController != null)
                charController.enabled = true;
            RenderSettings.skybox = skyBoxUnderWater;
        } else if (moveMode == MoveMode.Ski) {
            HUD_Ski1.SetActive(true);
            HUD_Ski2.SetActive(true);
            RenderSettings.skybox = skyBoxSki;
        }

        this.moveMode = moveMode;
    }

    public static Transform FindInChildrenInactive(Transform root, string path) {
        string[] parts = path.Split('/');
        Transform current = root;

        foreach (string part in parts) {
            current = Resources.FindObjectsOfTypeAll<Transform>()
                .FirstOrDefault(t => t.name == part && t.transform.parent == current);

            if (current == null)
                return null;
        }

        return current;
    }
}