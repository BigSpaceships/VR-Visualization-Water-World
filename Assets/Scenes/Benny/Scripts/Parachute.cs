using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Parachute : XRBaseInteractable
{
    [Header("Custom Fields")]
    [SerializeField] Transform selectedParent;
    [SerializeField] int attachSpeed;
    [SerializeField] XRInteractorLineVisual leftRay;
    [SerializeField] XRInteractorLineVisual rightRay;
    [SerializeField] SkiController leftSkiController;
    [SerializeField] SkiController rightSkiController;
    Collider[] cols;
    Transform myT;
    Rigidbody rb;
    Animator animator;
    bool selecting;

    protected override void Awake()
    {
        base.Awake();
        myT = transform;
        Transform parachute = myT.GetChild(0);
        Transform offset = parachute.GetChild(0);
        cols = new Collider[] { offset.GetChild(0).GetChild(0).GetComponent<Collider>(), offset.GetChild(1).GetComponent<Collider>() };
        rb = GetComponent<Rigidbody>();
        animator = parachute.GetComponent<Animator>();
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (selecting) return;
        selecting = true;
        base.OnSelectEntered(args);
        Pole pole = leftSkiController.attachedPole;
        if (pole != null) interactionManager.SelectExit(pole.selectInteractor, pole);
        pole = rightSkiController.attachedPole;
        if (pole != null) interactionManager.SelectExit(pole.selectInteractor, pole);
        StartCoroutine(Selected(new Vector3(0, 1.6f, 0.25f), Quaternion.Euler(0, -90, 0)));
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        if (selecting) return;
        selecting = true;
        base.OnSelectExited(args);
        StartCoroutine(Deselected(new Vector3(0, 0.2f, 2)));
    }

    IEnumerator Selected(Vector3 targetPos, Quaternion targetRot)
    {
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.None;
        for (int i = 0; i < 2; i++) cols[i].enabled = false;
        leftRay.enabled = rightRay.enabled = false;
        myT.parent = selectedParent;
        animator.SetTrigger("Unfurl");
        Vector3 pos = myT.localPosition;
        Quaternion rot = myT.localRotation;
        while (!EqualVectors(pos, targetPos) || !EqualVectors(rot.eulerAngles, targetRot.eulerAngles))
        {
            myT.localPosition = pos = Vector3.Lerp(pos, targetPos, Time.deltaTime * attachSpeed);
            myT.localRotation = rot = Quaternion.Slerp(rot, targetRot, Time.deltaTime * attachSpeed);
            yield return null;
        }
        myT.localPosition = targetPos;
        myT.localRotation = targetRot;
        Skier.paragliding = true;
        selecting = false;
    }
    IEnumerator Deselected(Vector3 targetPos)
    {
        Skier.paragliding = false;
        animator.SetTrigger("Close");
        Vector3 pos = myT.localPosition;
        while (!EqualVectors(pos, targetPos))
        {
            myT.localPosition = pos = Vector3.Lerp(pos, targetPos, Time.deltaTime * attachSpeed);
            yield return null;
        }
        myT.localPosition = targetPos;
        myT.parent = null;
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1) yield return null;
        for (int i = 0; i < 2; i++) cols[i].enabled = true;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        leftRay.enabled = rightRay.enabled = true;
        selecting = false;
    }

    bool EqualVectors(Vector3 vector, Vector3 target, float threshold = 0.1f)
    {
        if (Mathf.Abs(vector.x - target.x) < threshold && Mathf.Abs(vector.y - target.y) < threshold && Mathf.Abs(vector.z - target.z) < threshold) return true;
        return false;
    }
}