using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GrabAndRelease : MonoBehaviour
{
    private Rigidbody rb;
    private XRGrabInteractable grabInteractable;

    // Update is called once per frame
    void Update()
    {
    }

    private void Start() {
        // 获取 XRGrabInteractable 组件
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
        //rb.useGravity = false;

        // 订阅抓取事件
        grabInteractable.onSelectEntered.AddListener(OnGrabbed);
        grabInteractable.onSelectExited.AddListener(OnReleased);
    }

    private void OnGrabbed(XRBaseInteractor interactor) {
        //Invoke("ActivateGravity", 0.1f);
        return;

        // 用户开始抓取物体时的操作
        //Debug.Log("Object grabbed!");
        if (rb != null) {
            rb.useGravity = true; // Activate gravity
            rb.isKinematic = false;
            rb.WakeUp(); // 唤醒物体的物理模拟
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
        // 用户停止抓取物体时的操作
        //Debug.Log("Object released!");
        Invoke("ActivateGravity", .1f);
    }

    private void OnDestroy() {
        // 在销毁物体时取消订阅事件，防止内存泄漏
        grabInteractable.onSelectEntered.RemoveListener(OnGrabbed);
        grabInteractable.onSelectExited.RemoveListener(OnReleased);
    }
}
