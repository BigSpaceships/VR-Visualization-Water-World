using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.HID;

public class GameStart : MonoBehaviour {
    public Transform waypoint1;
    public Transform waypoint2;
    public Transform waypoint3;
    public Transform waypoint4;
    public Transform player;
    public Transform playerStartPoint;
    public float waypointActiveRadius = 3.0f;  //how may meters to reach(touch) waypoint

    private HUD_TextMessage HUD_textMessage;
    private HUD_WayPoint waypointController;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private bool originalFogEnabled;
    private FogMode originalFogMode;
    private Color originalFogColor;
    private float originalFogStartDistance;
    private float originalFogEndDistance;

    // Start is called before the first frame update
    void Start() {
        HUD_textMessage = Object.FindFirstObjectByType<HUD_TextMessage>();
        waypointController = Object.FindFirstObjectByType<HUD_WayPoint>();
        waypointController.ShowWaypoint(null); //clear waypoint


        GameObject xr = GamePublicV2.instance.xrOrigin;
        // ✅ 记录原始位置
        originalPosition = xr.transform.position;
        originalRotation = xr.transform.rotation;
        // ✅ 移动到区域起点
        xr.transform.SetPositionAndRotation(playerStartPoint.position, playerStartPoint.rotation);
        GamePublicV2.instance.setMoveMode(MoveMode.UnderWater);

        //forg control
        originalFogEnabled = RenderSettings.fog;
        originalFogMode = RenderSettings.fogMode;
        originalFogColor = RenderSettings.fogColor;
        originalFogStartDistance = RenderSettings.fogStartDistance;
        originalFogEndDistance = RenderSettings.fogEndDistance;
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color32(0x4E, 0xCA, 0xDD, 0xFF);
        RenderSettings.fogStartDistance = 5f;
        RenderSettings.fogEndDistance = 20f;

        //play task01 voice
        AudioSource TASK_D = GameObject.Find("TASK_DStart").GetComponent<AudioSource>(); //This is Tour Center, Welcome, David.You have successfully descended to the plan...
        TASK_D.Play();
        StartCoroutine(WaitForSeconds(TASK_D.clip.length, () => {
            Transform WayPoint1 = GameObject.Find("WayPoint1")?.transform;
            waypointController.ShowWaypoint(WayPoint1);
            AudioSource TASK_D = GameObject.Find("HUD_IncomingCord").GetComponent<AudioSource>();
            HUD_textMessage.ShowText("INCOMING COORDINATES RPF2K1\nWAYPOINT NAVIGATION SYSTEM: ACTIVE", TASK_D);
        }));
        //Invoke("ShowWelcomeText", 3f); // 3秒后调用 `ShowWelcomeText`
    }

    void OnDestroy() {
        GameObject xr = GamePublicV2.instance.xrOrigin;

        // ✅ 场景卸载时恢复位置
        if (xr != null)
            xr.transform.SetPositionAndRotation(originalPosition, originalRotation);

        RenderSettings.fog = originalFogEnabled;
        RenderSettings.fogMode = originalFogMode;
        RenderSettings.fogColor = originalFogColor;
        RenderSettings.fogStartDistance = originalFogStartDistance;
        RenderSettings.fogEndDistance = originalFogEndDistance;
    }

    // ✅ 一个通用的 WaitForSecondsWithCallback 实现
    IEnumerator WaitForSeconds(float time, System.Action callback) {
        yield return new WaitForSeconds(time);
        callback?.Invoke();
    }

    // Update is called once per frame
    void Update() {
        Transform currentWaypoint = waypointController.GetCurrentWaypointTarget();
        if (currentWaypoint != null) {
            float distance = Vector3.Distance(player.position, currentWaypoint.position);
            if (distance < waypointActiveRadius) {
                waypointController.ShowWaypoint(null);  //close current waypoint
                if (currentWaypoint == waypoint1) {
                    onWaypoint1Reached();
                } else if (currentWaypoint == waypoint2) {
                    onWaypoint2Reached();
                } else if (currentWaypoint == waypoint3) {
                    onWaypoint3Reached();
                } else if (currentWaypoint == waypoint4) {
                    onWaypoint4Reached();
                }
            }
        }
    }

    void onWaypoint1Reached() {
        HUD_textMessage.ShowText("WAYPOINT RPF2K1\nSTATUS: REACHED", null);
        AudioSource TASK_D = GameObject.Find("TASK_D01").GetComponent<AudioSource>();
        TASK_D.Play();
        StartCoroutine(WaitForSeconds(TASK_D.clip.length, () => {
            AudioSource TASK_D = GameObject.Find("HUD_IncomingCord").GetComponent<AudioSource>(); 
            HUD_textMessage.ShowText("INCOMING COORDINATES RPF4K2\nWAYPOINT NAVIGATION SYSTEM: ACTIVE", TASK_D);
            waypointController.ShowWaypoint(waypoint2);
        }));
    }

    void onWaypoint2Reached() {
        HUD_textMessage.ShowText("WAYPOINT RPF4K2\nSTATUS: REACHED", null);
        AudioSource TASK_D = GameObject.Find("TASK_D02").GetComponent<AudioSource>();
        TASK_D.Play();
        StartCoroutine(WaitForSeconds(TASK_D.clip.length, () => {
            AudioSource TASK_D = GameObject.Find("HUD_IncomingCord").GetComponent<AudioSource>();
            HUD_textMessage.ShowText("INCOMING COORDINATES RPF6K3\nWAYPOINT NAVIGATION SYSTEM: ACTIVE", TASK_D);
            waypointController.ShowWaypoint(waypoint3);
        }));
    }

    void onWaypoint3Reached() {
        HUD_textMessage.ShowText("WAYPOINT RPF6K3\nSTATUS: REACHED", null);
        AudioSource TASK_D = GameObject.Find("TASK_D03").GetComponent<AudioSource>();
        TASK_D.Play();
        StartCoroutine(WaitForSeconds(TASK_D.clip.length, () => {
            AudioSource TASK_D = GameObject.Find("HUD_IncomingCord").GetComponent<AudioSource>();
            HUD_textMessage.ShowText("INCOMING COORDINATES RPF8K4\nWAYPOINT NAVIGATION SYSTEM: ACTIVE", TASK_D);
            waypointController.ShowWaypoint(waypoint4);
        }));
    }

    void onWaypoint4Reached() {
        HUD_textMessage.ShowText("WAYPOINT RPF8K4\nSTATUS: REACHED", null);
        AudioSource TASK_D = GameObject.Find("TASK_D04").GetComponent<AudioSource>();
        TASK_D.Play();
    }
}
