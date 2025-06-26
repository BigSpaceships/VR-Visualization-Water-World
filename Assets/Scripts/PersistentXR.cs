using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistentXR : MonoBehaviour {
    public static PersistentXR Instance;
    private void Awake() {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
