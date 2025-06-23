using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TeleportEffect : MonoBehaviour
{
    // Start is called before the first frame update
    void OnEnable()
    {
        GetComponent<TeleportationArea>().activated.AddListener(delegate { Manager.instance.Teleport(); });
    }
    void OnDisable()
    {
        GetComponent<TeleportationArea>().activated.RemoveListener(delegate { Manager.instance.Teleport(); });
    }
}
