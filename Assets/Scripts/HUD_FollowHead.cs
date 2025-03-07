using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUD_FollowHead : MonoBehaviour {
    public Transform head;  // °ó¶¨ Main Camera
    public Vector3 offset = new Vector3(0, -0.1f, 0.5f); // Î»ÖÃÆ«ÒÆ

    void LateUpdate() {
        if (head) {
            transform.position = head.position + head.rotation * offset;
            transform.rotation = Quaternion.LookRotation(transform.position - head.position);
        }
    }
}