using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Skis : XRBaseInteractable
{
    [SerializeField] int alignStrength = 5;
    [SerializeField] int maxAlignAngle = 60;
    [SerializeField] LayerMask groundMask;
    [SerializeField] Transform skiParent;
    [SerializeField] Vector3 attach;
    Transform myT;
    Rigidbody rb;
    bool selected;

    protected override void Awake()
    {
        base.Awake();
        myT = transform;
    }

    void Update()
    {
        if (!selected) return;
        //Ski stabilization
        if (Physics.Raycast(myT.position + myT.up * 0.5f, -myT.up, out RaycastHit hit, 0.6f, groundMask))
        {
            Vector3 groundUp = hit.normal;
            if (Vector3.Angle(myT.up, groundUp) < maxAlignAngle)
            {
                Quaternion targetRotation = Quaternion.FromToRotation(myT.up, groundUp) * myT.rotation;
                myT.rotation = Quaternion.Slerp(myT.rotation, targetRotation, Time.deltaTime * alignStrength);
            }
        }
        else myT.localRotation = Quaternion.Slerp(myT.localRotation, Quaternion.identity, Time.deltaTime * alignStrength);
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        if (myT.parent == null)
        {
            if (rb != null) Destroy(rb);
            myT.parent = skiParent;
            myT.SetLocalPositionAndRotation(attach, Quaternion.identity);
            interactionManager.SelectExit(args.interactorObject, this);
            selected = true;
            Skier.attachedSkis += 1;
            if (Skier.attachedSkis == 1)
            {
                Skier.rb.constraints = RigidbodyConstraints.None;
                Skier.rb.drag = 0;
            }
        }
            else
            {
                selected = false;
                Skier.attachedSkis -= 1;
                if (Skier.attachedSkis == 0)
                {
                    Skier.rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                    Skier.rb.drag = 3;
                }
                myT.parent = null;
                rb = myT.AddComponent<Rigidbody>();
            }
    }
}
