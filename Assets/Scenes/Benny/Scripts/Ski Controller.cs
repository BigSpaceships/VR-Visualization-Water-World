using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SkiController : ActionBasedController
{
    [Header("Custom Fields")]
    [SerializeField] Transform controllerModel;
    [SerializeField] Transform handModel;
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

    public void SwitchController(bool hands)
    {
        //Toggle hands/controller graphic
        if (hands)
        {
            controllerModel.gameObject.SetActive(false);
            handModel.gameObject.SetActive(true);
            model = handModel;
            animator = model.GetComponent<Animator>();
        }
        else
        {
            handModel.gameObject.SetActive(false);
            controllerModel.gameObject.SetActive(true);
            model = controllerModel;
            animator = null;
        }
        if (attachedPole != null) attachedPole.SwitchController();
    }

    public void Animate(string trigger)
    {
        //Hand grabbing animation
        if (animator != null) animator.SetTrigger(trigger);
    }
}
