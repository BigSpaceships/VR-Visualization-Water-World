using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
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

        GameObject rigObject = GameObject.Find("XR Origin (XR Rig)");
        playerRb = xrOrigin.GetComponent<Rigidbody>();
        charController = rigObject.GetComponent<CharacterController>();
        seaScooter = GameObject.Find("SeaScooter");

        GameInit();
    }

    private void GameInit() {
        setMoveMode(MoveMode.Ground);
    }

    public void setMoveMode(MoveMode moveMode) {
        if (moveMode == this.moveMode) return;
        
        //release old mode
        if (this.moveMode == MoveMode.Ground) {

        } else if (this.moveMode == MoveMode.UnderWater) {

        }

        //initialize new mode
        if (moveMode == MoveMode.Ground) {
            playerRb.isKinematic = false;
            playerRb.useGravity = true;
            playerRb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            if (charController != null)
                charController.enabled = false;  // �����ϣ�� CharacterController ģʽ��δ����չ�����Ը�Ϊ true
            seaScooter.SetActive(false);
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


    
}