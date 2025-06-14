using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SkiController : ActionBasedController
{
    [SerializeField] Transform controllerModel;
    [SerializeField] bool hands;
    Transform myT;
    Animator animator;

    /*protected override void UpdateTrackingInput(XRControllerState controllerState)
    {
        base.UpdateTrackingInput(controllerState);
        Vector3 pos = myT.localPosition;
        pos.x = Mathf.Clamp(pos.x, -0.4f, 0.4f);
        pos.z = Mathf.Clamp(pos.z, -0.4f, 0.4f);
        myT.localPosition = pos;
    }*/

    protected override void Awake()
    {
        base.Awake();
        myT = transform;
        if (hands)
        {
            controllerModel.gameObject.SetActive(false);
            animator = model.GetComponent<Animator>();
            Pole.rightAttach = new Vector3(0.03f, -1.28f, -0.03f);
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
