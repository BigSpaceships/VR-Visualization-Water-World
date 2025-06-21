using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.Rendering.Universal;

public class HUD_WayPoint : MonoBehaviour {
    public Transform target;  // 目标物体
    public RectTransform wayPoint;  // HUD上的准星 (UI Image)
    public RectTransform arrowIndicator; // 屏幕外指示箭头
    public Camera mainCamera;  // 玩家主摄像机
    public RectTransform hudRect;  // HUD rectangle
    public TextMeshProUGUI distanceText; // 显示目标距离的文字

    private Vector2 screenBorder = new Vector2(5, 6);
    private float HUDCanvasZoom = 80f; //屏幕距离玩家远近产生的缩放系数


    private void Start() {
        GameObject persistentXR = GameObject.Find("PersistentXR");
        if (persistentXR == null) return;

        Transform xrOrigin = persistentXR.transform.Find("XR Origin/XR Origin (XR Rig)/Camera Offset");
        if (xrOrigin == null) {
            Debug.LogError("XR Origin structure not found under PersistentXR.");
            return;
        }

        // 自动绑定关键引用
        mainCamera = xrOrigin.Find("Main Camera")?.GetComponent<Camera>();
        wayPoint = GamePublicV2.FindInChildrenInactive(GamePublicV2.instance.HUD_UnderWater.transform, "Image_WayPoint")?.GetComponent<RectTransform>();
        arrowIndicator = GamePublicV2.FindInChildrenInactive(GamePublicV2.instance.HUD_UnderWater.transform, "WayPointArrow")?.GetComponent<RectTransform>();
        hudRect = GamePublicV2.FindInChildrenInactive(GamePublicV2.instance.HUD_UnderWater.transform, "Image_BG")?.GetComponent<RectTransform>();
        distanceText = GamePublicV2.FindInChildrenInactive(GamePublicV2.instance.HUD_UnderWater.transform, "Image_WayPoint/Text_WayPointDistance")?.GetComponent<TMPro.TextMeshProUGUI>();
    }

    /// <summary>
    /// ✅ 对外接口：设置 WayPoint 目标
    /// </summary>
    /// <param name="newTarget">目标物体，传 null 关闭 WayPoint</param>
    public void ShowWaypoint(Transform newTarget) {
        
        target = newTarget;

        if (target == null) {
            wayPoint.gameObject.SetActive(false);
            arrowIndicator.gameObject.SetActive(false);
            distanceText.gameObject.SetActive(false);
            Debug.Log($"showwaypoint:null");
        } else {
            Debug.Log($"showwaypoint: {newTarget.name} position:{newTarget.position}");
            wayPoint.gameObject.SetActive(true);
            arrowIndicator.gameObject.SetActive(true);
            distanceText.gameObject.SetActive(true);
        }
    }

    public Transform GetCurrentWaypointTarget() {
        if (wayPoint.gameObject.activeSelf) {
            return target;
        } else return null;
    }

    void Update() {
        if (target == null || wayPoint == null || mainCamera == null)
            return;

        // 1️⃣ 获取目标在屏幕上的位置
        Vector3 screenPos = mainCamera.WorldToScreenPoint(target.position);
        bool isBehind = screenPos.z < 0; // 目标是否在玩家背后

        // 2️⃣ 计算目标距离
        float distance = Vector3.Distance(mainCamera.transform.position, target.position);
        distanceText.text = $"{distance:F1}m"; // 显示到 0.1m 精度

        // 3️⃣ 计算目标的屏幕方向（归一化）
        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        Vector3 directionToTarget = (screenPos - screenCenter).normalized;

        // 4️⃣ 限制准星在 HUD Canvas 内
        float halfWidth = hudRect.rect.width * hudRect.localScale.x / 2 - screenBorder.x;
        float halfHeight = hudRect.rect.height * hudRect.localScale.y / 2 - screenBorder.y;

        Vector2 uiPosition;

        bool showArrow = false;
        if (isBehind) {
            // 目标在背后，计算准星贴在最近的屏幕边缘
            if (Mathf.Abs(directionToTarget.x) > Mathf.Abs(directionToTarget.y)) {
                // X方向更大，贴在左右边界
                uiPosition.x = (directionToTarget.x < 0) ? halfWidth : -halfWidth;
                uiPosition.y = directionToTarget.y * halfHeight;
                showArrow = true;
            } else {
                // Y方向更大，贴在上下边界
                uiPosition.y = (directionToTarget.y > 0) ? halfHeight : -halfHeight;
                uiPosition.x = directionToTarget.x * halfWidth;
                showArrow = true;
            }
        } else {
            // 目标在前方，正常转换坐标
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                hudRect, screenPos, mainCamera, out uiPosition);
            uiPosition.x *= hudRect.localScale.x;
            uiPosition.y *= hudRect.localScale.y;
            if (uiPosition.x < -halfWidth) {
                uiPosition.x = -halfWidth;
                showArrow = true;
            };
            if (uiPosition.x > halfWidth) {
                uiPosition.x = halfWidth;
                showArrow = true;
            };
            if (uiPosition.y < -halfHeight) {
                uiPosition.y = -halfHeight;
                showArrow = true;
            };
            if (uiPosition.y > halfHeight) {
                uiPosition.y = halfHeight;
                showArrow = true;
            };
        }

        // 5️⃣ 更新准星位置
        wayPoint.anchoredPosition = uiPosition;

        // 6️⃣ 目标超出视野时，显示指示箭头
        if (arrowIndicator != null) {
            if (showArrow) {
                arrowIndicator.gameObject.SetActive(true);
                arrowIndicator.anchoredPosition = uiPosition;
                arrowIndicator.transform.LookAt(target.transform.position);
                /*
                arrowIndicator.gameObject.SetActive(true);
                // 计算箭头指向目标的角度
                float angle = Mathf.Atan2(uiPosition.y, uiPosition.x) * Mathf.Rad2Deg;
                arrowIndicator.anchoredPosition = uiPosition;
                arrowIndicator.rotation = Quaternion.Euler(0, 0, angle);
                */
            } else {
                arrowIndicator.gameObject.SetActive(false);
            }
        }
    }
      
}
