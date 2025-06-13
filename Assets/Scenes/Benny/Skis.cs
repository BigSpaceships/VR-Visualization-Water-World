using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Skis : XRBaseInteractable
{
    [SerializeField] int alignStrength = 5;
    [SerializeField] int maxAlignAngle = 60;
    [SerializeField] LayerMask groundMask;
    Transform myT;

    protected override void Awake()
    {
        base.Awake();
        myT = transform;
    }

    void Update()
    {
        //Ski stabilization
        if (Physics.Raycast(myT.position + myT.TransformPoint(new Vector3(0, 0.5f, 0.7f)), -myT.up, out RaycastHit hit, 0.7f, groundMask))
        {
            Vector3 groundUp = hit.normal;
            if (Vector3.Angle(myT.up, groundUp) < maxAlignAngle)
            {
                Quaternion targetRotation = Quaternion.FromToRotation(myT.up, groundUp) * myT.rotation;
                myT.rotation = Quaternion.Slerp(myT.rotation, targetRotation, Time.deltaTime * alignStrength);
            }
        }
        else
        {
            myT.localRotation = Quaternion.Slerp(myT.localRotation, Quaternion.identity, Time.deltaTime * alignStrength);
        }
    }
}
