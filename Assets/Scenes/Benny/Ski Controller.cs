using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SkiController : ActionBasedController
{
    [SerializeField] Transform controllerModel;
    [SerializeField] bool hands;
    Animator animator;

    /*protected override void UpdateTrackingInput(XRControllerState controllerState)
    {
        base.UpdateTrackingInput(controllerState);
        if (controllerState == null) return;
        Vector3 pos = controllerState.position;
        pos.x = Mathf.Clamp(pos.x, -0.5f, 0.5f);
        pos.z = Mathf.Clamp(pos.z, -0.5f, 0.5f);
        controllerState.position = pos;
    }*/

    protected override void Awake()
    {
        base.Awake();
        if (hands)
        {
            controllerModel.gameObject.SetActive(false);
            animator = model.GetComponent<Animator>();
            Pole.rightAttach = new Vector3(0.03f, -0.65f, -0.03f);
        }
        else
        {
            model.gameObject.SetActive(false);
            model = controllerModel;
        }
    }

    public void Animate(string trigger)
    {
        if (!hands) return;
        animator.SetTrigger(trigger);
    }
}
