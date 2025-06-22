using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistentXR : MonoBehaviour
{
    public static PersistentXR Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
}
