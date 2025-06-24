using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class disableXR : MonoBehaviour {
    void Awake() {
        if (GameObject.Find("PersistentXR") != null) { // ��ǰ������XR������
            this.gameObject.SetActive(false);
        }
    }
}
