using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwimTrans : MonoBehaviour {
    public Transform swimmingRing;
    public Transform skiHandL;
    public Transform skiHandR;
    public Transform skiFoot;

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public void attachSwimmingRing() {
        GameObject player;
        if (GamePublicV2.instance.checkVRActive()) {
            player = GameObject.Find("PersistentXR/XR Origin/XR Origin (XR Rig)");
        } else {
            player = GameObject.Find("PersistentXR/XR Origin"); 
        }
        if (player == null) return;

        swimmingRing.SetParent(player.transform);
        swimmingRing.localPosition = Vector3.zero;
    }

    public void attachSki() {
        GameObject player;
        if (GamePublicV2.instance.checkVRActive()) {
            player = GameObject.Find("PersistentXR/XR Origin/XR Origin (XR Rig)");
        } else {
            player = GameObject.Find("PersistentXR/XR Origin");
        }
        if (player != null) {
            skiFoot.SetParent(player.transform);
            skiFoot.transform.localPosition = Vector3.zero;
        }

        GameObject controllerLeft = GameObject.Find("PersistentXR/XR Origin/XR Origin (XR Rig)/Camera Offset/Left Controller");
        GameObject controllerRight = GameObject.Find("PersistentXR/XR Origin/XR Origin (XR Rig)/Camera Offset/Right Controller");
        if (controllerLeft != null) {
            skiHandL.SetParent(controllerLeft.transform);
            skiHandL.localPosition = Vector3.zero;
        }
        if (controllerRight != null) {
            skiHandR.SetParent(controllerRight.transform);
            skiHandR.localPosition = Vector3.zero;
        }
    }

}
