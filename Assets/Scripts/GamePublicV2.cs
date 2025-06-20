using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;
public enum MoveMode {
    None,
    Ground,
    UnderWater
}

public class GamePublicV2 : MonoBehaviour {
    public static GamePublicV2 instance;

    public bool publicAccessSamples = false; //in other script use this to access:  GamePublicV2.instance.publicAccessSamples
    public MoveMode moveMode = MoveMode.None;
    public GameObject xrOrigin;

    private Rigidbody playerRb;
    private CharacterController charController;
    private GameObject seaScooter;

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

        GameInit();
    }

    private void GameInit() {
        setMoveMode(MoveMode.Ground);
    }

    public bool checkVRActive() {
        bool active = XRSettings.isDeviceActive;
        var simHMD = InputSystem.GetDevice<XRSimulatedHMD>();
        if (simHMD != null) {
            active = true;
        }
        return active;
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
            } else {
                charController.enabled = false;
                playerRb.isKinematic = false;
                playerRb.useGravity = true;
                playerRb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
                seaScooter.SetActive(false);
            }
        } else if (moveMode == MoveMode.UnderWater) {
            playerRb.isKinematic = true;   // 禁用刚体物理
            playerRb.useGravity = false;   // 水下无重力
            playerRb.constraints = RigidbodyConstraints.None; // 自由转动
            seaScooter.SetActive(true);
            if (charController != null)
                charController.enabled = true;
        }

        this.moveMode = moveMode;
    }


    
}