using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class disableXR : MonoBehaviour {
    void Awake() {
        if (GameObject.Find("PersistentXR") != null) { // ��ǰ������XR������
            this.gameObject.SetActive(false);
        }
    }

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
}
