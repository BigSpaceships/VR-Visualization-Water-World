using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MoveMode {
    None,
    OnEarth,
    UnderWater
}

public class GamePublicV2 : MonoBehaviour {
    public static GamePublicV2 instance;

    public bool publicAccessSamples = false; //in other script use this to access:  GamePublicV2.instance.publicAccessSamples
    public MoveMode moveMode = MoveMode.None;

    private Rigidbody playerRb;
    private CharacterController charController;

    private void Awake() {
        if (instance == null) {
            instance = this;
        }

        GameObject rigObject = GameObject.Find("XR Origin (XR Rig)");
        GameObject bodyObject = GameObject.Find("XR Origin");
        playerRb = bodyObject.GetComponent<Rigidbody>();
        charController = rigObject.GetComponent<CharacterController>();

        GameInit();
    }

    private void GameInit() {
        setMoveMode(MoveMode.OnEarth);
    }

    public void setMoveMode(MoveMode moveMode) {
        if (moveMode == this.moveMode) return;
        
        //release old mode
        if (this.moveMode == MoveMode.OnEarth) {

        } else if (this.moveMode == MoveMode.UnderWater) {

        }

        //initialize new mode
        if (moveMode == MoveMode.OnEarth) {
            playerRb.isKinematic = false;
            playerRb.useGravity = true;
            playerRb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            if (charController != null)
                charController.enabled = false;  // �����ϣ�� CharacterController ģʽ��δ����չ�����Ը�Ϊ true
        } else if (moveMode == MoveMode.UnderWater) {
            playerRb.isKinematic = true;   // ���ø�������
            playerRb.useGravity = false;   // ˮ��������
            playerRb.constraints = RigidbodyConstraints.None; // ����ת��
            if (charController != null)
                charController.enabled = true;
        }

        this.moveMode = moveMode;
    }


    
}