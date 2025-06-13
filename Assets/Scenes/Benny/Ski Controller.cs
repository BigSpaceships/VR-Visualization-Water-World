using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SkiController : ActionBasedController
{
    
    /*protected override void UpdateTrackingInput(XRControllerState controllerState)
    {
        base.UpdateTrackingInput(controllerState);
        if (controllerState == null) return;
        Vector3 pos = controllerState.position;
        pos.x = Mathf.Clamp(pos.x, -0.5f, 0.5f);
        pos.z = Mathf.Clamp(pos.z, -0.5f, 0.5f);
        controllerState.position = pos;
    }*/
}
