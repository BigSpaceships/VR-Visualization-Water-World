using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUD_FloatingFollow : MonoBehaviour {
    public Transform cameraTransform;  // 主摄像机（通常是 VR 头盔相机）
    public float followSpeed = 2.0f;   // HUD 位置跟随速度
    public float floatingIntensity = 0.05f; // 漂浮幅度（值越大漂浮越明显）
    public float floatingSpeed = 1.0f; // 漂浮速度（值越大漂浮越快）
    public Vector3 positionLimits = new Vector3(0.04f, 0.02f, 0.01f);

    private Vector3 targetPosition; // 目标位置
    private Vector3 initLocalPos; //initial local position
    private float distanceFromCamera = 0.5f; // HUD 与摄像机的固定前方距离

    void Start() {
        if (cameraTransform == null) {
            cameraTransform = Camera.main.transform; // 默认使用主摄像机
        }
        targetPosition = transform.position; // 记录初始位置
        initLocalPos = transform.localPosition;

        distanceFromCamera = transform.localPosition.z;
    }

    void Update() {
        if (cameraTransform == null) return;

        // 1️⃣ 计算目标位置（让 HUD 稍微滞后跟随摄像机）
        Vector3 desiredPosition = cameraTransform.TransformPoint(initLocalPos); // HUD 距离头部 0.5m

        Vector3 localOffset = cameraTransform.InverseTransformPoint(targetPosition);

        // 🚀 **限制 HUD 在玩家视野范围内**
        localOffset.x = Mathf.Max(Mathf.Min(localOffset.x, initLocalPos.x + positionLimits.x), initLocalPos.x - positionLimits.x);
        localOffset.y = Mathf.Max(Mathf.Min(localOffset.y, initLocalPos.y + positionLimits.y), initLocalPos.y - positionLimits.y);
        localOffset.z = Mathf.Max(Mathf.Min(localOffset.z, initLocalPos.z + positionLimits.z), initLocalPos.z - positionLimits.z);

        targetPosition = cameraTransform.TransformPoint(localOffset);
        targetPosition = Vector3.Lerp(targetPosition, desiredPosition, followSpeed * Time.deltaTime);

        // 2️⃣ 计算 HUD 漂浮偏移
        float offsetX = (Mathf.PerlinNoise(Time.time * floatingSpeed, 0) - distanceFromCamera) * floatingIntensity;
        float offsetY = (Mathf.PerlinNoise(0, Time.time * floatingSpeed) - distanceFromCamera) * floatingIntensity;
        Vector3 floatingOffset = new Vector3(offsetX, offsetY, 0);

        // 3️⃣ 更新 HUD 位置（跟随摄像机 + 漂浮效果）
        transform.position = targetPosition + floatingOffset;

        // 4️⃣ 确保 HUD 始终朝向摄像机
        transform.rotation = Quaternion.LookRotation(transform.position - cameraTransform.position);
    }
    
}
