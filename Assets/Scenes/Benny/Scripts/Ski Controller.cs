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

    protected override void Awake()
    {
        base.Awake();
        //myT = transform;
    }

    /*protected override void UpdateTrackingInput(XRControllerState controllerState)
    {
        //Clamp controller position
        Vector3 pos = cam.InverseTransformPoint(myT.position);
        pos.x = Mathf.Clamp(pos.x, -1, 1);
        pos.y = Mathf.Clamp(pos.y, -1, 1);
        pos.z = Mathf.Clamp(pos.z, -1, 1);
        controllerState.position = pos;
    }*/

    public void SwitchController(bool hands, bool selected = false)
    {
        if (attachedPole != null)
        {
            selected = true;
            attachedPole.SwitchController();
        }
        
        //Toggle hands/controller graphic
        if (hands)
        {
            controllerModel.gameObject.SetActive(false);
            handModel.gameObject.SetActive(true);
            model = handModel;
            animator = model.GetComponent<Animator>();
            if (selected) Animate("Select");
        }
        else
        {
            handModel.gameObject.SetActive(false);
            controllerModel.gameObject.SetActive(true);
            model = controllerModel;
            animator = null;
        }
    }

    public void Animate(string trigger)
    {
        //Hand grabbing animation
        if (animator != null) animator.SetTrigger(trigger);
    }
}
