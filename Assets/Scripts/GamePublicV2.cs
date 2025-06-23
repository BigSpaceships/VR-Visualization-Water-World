using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;
using UnityEngine.XR.Interaction.Toolkit.UI;
public enum MoveMode {
    None,
    Ground,
    UnderWater
}

public enum ControllerName {
    None,
    Main,
    A2
}

public class GamePublicV2 : MonoBehaviour {
    public static GamePublicV2 instance;

    public bool publicAccessSamples = false; //in other script use this to access:  GamePublicV2.instance.publicAccessSamples
    public MoveMode moveMode = MoveMode.None;
    public GameObject xrOrigin;

    public GameObject HUD_UnderWater;

    private Rigidbody playerRb;
    private CharacterController charController;
    private GameObject seaScooter;
    private CapsuleCollider xrOriginCollider;


    private GameObject controllerA2Left;
    private GameObject controllerA2Right;
    private GameObject controllerMainLeft;
    private GameObject controllerMainRight;


    private void Awake() {
        if (instance == null) {
            instance = this;
        }

        GameObject persistentXR = GameObject.Find("PersistentXR");
        if (persistentXR == null) {
            Debug.LogError("can not find GameObject: PersistentXR");
            return;
        }

        GameObject XROrigin = persistentXR.transform.Find("XR Origin").gameObject;
        GameObject XROriginRig = persistentXR.transform.Find("XR Origin/XR Origin (XR Rig)").gameObject;
        playerRb = xrOrigin.GetComponent<Rigidbody>();
        charController = XROriginRig.GetComponent<CharacterController>();
        seaScooter = GameObject.Find("SeaScooter");
        xrOriginCollider = XROrigin.GetComponent<CapsuleCollider>();

        controllerA2Left = XROriginRig.transform.Find("Camera Offset/Left Controller A2").gameObject;
        controllerA2Right = XROriginRig.transform.Find("Camera Offset/Right Controller A2").gameObject;
        controllerMainLeft = XROriginRig.transform.Find("Camera Offset/Left Controller").gameObject;
        controllerMainRight = XROriginRig.transform.Find("Camera Offset/Right Controller").gameObject;

        HUD_UnderWater = XROriginRig.transform.Find("Camera Offset/Main Camera/Canvas_HUD").gameObject;

        GameInit();
    }

    private void GameInit() {
        setMoveMode(MoveMode.Ground);
        setController(ControllerName.Main);
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


    private IEnumerator ResetAndSwitch(ControllerName set) {
        DisableAllInteractors();
        yield return null;

        // �������п�����
        controllerMainLeft?.SetActive(false);
        controllerMainRight?.SetActive(false);
        controllerA2Left?.SetActive(false);
        controllerA2Right?.SetActive(false);

        // ����ָ����������
        GameObject left = null, right = null;
        if (set == ControllerName.Main) {
            left = controllerMainLeft;
            right = controllerMainRight;
        } else if (set == ControllerName.A2) {
            left = controllerA2Left;
            right = controllerA2Right;
        }

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

        // ���ÿ��ӻ�����
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
        if (moveMode == MoveMode.Ground) {
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
        } else if (moveMode == MoveMode.UnderWater) {
            playerRb.isKinematic = true;   // ���ø�������
            playerRb.useGravity = false;   // ˮ��������
            playerRb.constraints = RigidbodyConstraints.None; // ����ת��
            seaScooter.SetActive(true);
            if (charController != null)
                charController.enabled = true;
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