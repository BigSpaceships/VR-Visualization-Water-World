using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SkiController : ActionBasedController
{
    [SerializeField] Transform controllerModel;
    [SerializeField] Transform handModel;
    [SerializeField] bool hands;
    Animator animator;
    public Pole attachedPole;
    public Skis attachedSki;

    /*protected override void UpdateTrackingInput(XRControllerState controllerState)
    {
        //Clamp controller position
        base.UpdateTrackingInput(controllerState);
        Vector3 pos = myT.localPosition;
        pos.x = Mathf.Clamp(pos.x, -0.4f, 0.4f);
        pos.z = Mathf.Clamp(pos.z, -0.4f, 0.4f);
        myT.localPosition = pos;
    }*/
    

    protected override void Awake()
    {
        base.Awake();
        if (hands)
        {
            controllerModel.gameObject.SetActive(false);
            model = handModel;
            animator = model.GetComponent<Animator>();
            Pole.rightAttach = new Vector3(0.035f, -1.28f, -0.03f);
            Pole.leftAttach = new Vector3(-0.035f, -1.28f, -0.03f);
        }
        else
        {
            handModel.gameObject.SetActive(false);
            model = controllerModel;
            Pole.rightAttach = Pole.leftAttach = new Vector3(0, -1.3f, 0.05f);
        }
    }

    public void SwitchController()
    {
        //Toggle hands/controller graphic
        hands = !hands;
        if (hands)
        {
            controllerModel.gameObject.SetActive(false);
            handModel.gameObject.SetActive(true);
            model = handModel;
            animator = model.GetComponent<Animator>();
            Pole.rightAttach = new Vector3(0.035f, -1.28f, -0.03f);
            Pole.leftAttach = new Vector3(-0.035f, -1.28f, -0.03f);
        }
        else
        {
            handModel.gameObject.SetActive(false);
            controllerModel.gameObject.SetActive(true);
            model = controllerModel;
            Pole.rightAttach = Pole.leftAttach = new Vector3(0, -1.3f, 0.05f);
        }
        if (attachedPole != null) attachedPole.SwitchController();
    }

    public void Animate(string trigger)
    {
        //Hand grabbing animation
        if (!hands) return;
        animator.SetTrigger(trigger);
    }
}
