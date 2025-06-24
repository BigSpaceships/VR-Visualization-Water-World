using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GrabAndRelease : MonoBehaviour
{
    private Rigidbody rb;
    private XRGrabInteractable grabInteractable;

    private void Start() {
        // ��ȡ XRGrabInteractable ���
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
        //rb.useGravity = false;

        // ����ץȡ�¼�
        grabInteractable.onSelectEntered.AddListener(OnGrabbed);
        grabInteractable.onSelectExited.AddListener(OnReleased);
    }

    private void OnGrabbed(XRBaseInteractor interactor) {
        //Invoke("ActivateGravity", 0.1f);
        return;

        // �û���ʼץȡ����ʱ�Ĳ���
        //Debug.Log("Object grabbed!");
        if (rb != null) {
            rb.useGravity = true; // Activate gravity
            rb.isKinematic = false;
            rb.WakeUp(); // �������������ģ��
            Debug.Log("gravity triggered");
        } else {
            Debug.Log("no gravity triggered");
        }
    }


    private void ActivateGravity() {
        if (rb != null) {
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.WakeUp();
        }
    }

    private void OnReleased(XRBaseInteractor interactor) {
        // �û�ֹͣץȡ����ʱ�Ĳ���
        //Debug.Log("Object released!");
        Invoke("ActivateGravity", .1f);
    }

    private void OnDestroy() {
        // ����������ʱȡ�������¼�����ֹ�ڴ�й©
        grabInteractable.onSelectEntered.RemoveListener(OnGrabbed);
        grabInteractable.onSelectExited.RemoveListener(OnReleased);
    }
}
