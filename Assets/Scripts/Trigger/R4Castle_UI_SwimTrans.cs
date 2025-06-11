using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwimTrans : MonoBehaviour {
    public Transform swimmingRing;

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public void attachSwimmingRing() {
        GameObject player = GameObject.Find("PersistentXR/XR Origin");
        if (player == null) return;

        swimmingRing.SetParent(player.transform);
        swimmingRing.localPosition = Vector3.zero;
    }

}
