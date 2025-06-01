using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistentXR : MonoBehaviour
{
    private void Awake() {
        DontDestroyOnLoad(gameObject);
    }
}
