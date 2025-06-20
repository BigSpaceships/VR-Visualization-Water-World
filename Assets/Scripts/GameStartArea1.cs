using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.SceneManagement;
using static UnityEngine.GraphicsBuffer;

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
        HUD_textMessage = Object.FindFirstObjectByType<HUD_TextMessage>(FindObjectsInactive.Include);
        waypointController = Object.FindFirstObjectByType<HUD_WayPoint>(FindObjectsInactive.Include);
        waypointController.ShowWaypoint(null); //clear waypoint


        GameObject persistentXR = GameObject.Find("PersistentXR");
        if (persistentXR != null) {
            player = persistentXR.transform.Find("XR Origin/XR Origin (XR Rig)/Camera Offset/Main Camera");
            if (player == null) {
                Debug.LogError("can not find persistentXR/XR Origin/XR Origin (XR Rig)/Camera Offset/Main Camera");
            }
        }

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

        StartCoroutine(TaskStart());
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


    /* 启动：
     * 这里是旅游中心，恭喜您已经成为我们的VIP用户，下面我将为您提供免费的导游服务，请跟随坐标并启动水下推进器，沿着指引通道前往目标位置，准备揭开海底文明遗迹的神秘面纱。
     * 
     * waypoint1:
     * 尊敬的潜水探险者，千年前这座古城因海啸沉没。您已到达入口走廊，请沿着珊瑚覆盖的石制走廊前行，请注意氧气用量，保持均匀呼吸。
     * 
     * waypoint2:
     * 尊敬的潜水探险者，您已抵达遗迹入口城门附近。这座宏伟的建筑经受住了时间的考验，如今已被深海缓慢吞噬。依稀可见的雕刻与风化的符文仍留存其上，暗示着这里曾经繁荣的文明。
     * 
     * waypoint3:
     * 尊敬的VIP潜水探险者，您眼前即是水下城堡的核心区域，本次旅程将在此结束，请做好上浮准备。感谢您选择我们的服务，只需支付 99,999 美元，便可升级为 VIP2 尊享会员，免费开启城堡内部深度探索之旅。
     * 
     * waypoint4:结束
     * 启动减压程序，准备上浮。
     * 
     * Start:
       This is Tour Center, congratulations, you’re now one of our VIP members! From here on out, I’ll be your personal guide at no extra cost. Follow the coordinates we’ve sent you and fire up your underwater thrusters. Swim through the marked passage to your destination, and get ready to uncover the mysteries of the sunken civilization.

        Waypoint 1:
        Hey, adventurer—about a thousand years ago, this ancient city was swallowed by a massive tsunami. You’ve reached the entrance corridor. Glide along the coral-covered stone walkway, keep an eye on your oxygen gauge, and breathe evenly.

        Waypoint 2:
        You’re now at the ruins’ main gate. This grand arch has stood the test of time, but the sea is slowly reclaiming it. Faint carvings and weathered runes still peek through, hinting at the city’s former glory. Take a moment to look around before moving on.

        Waypoint 3:
        VIP explorer, you’ve arrived at the heart of the underwater castle—our tour ends here. Please prepare to ascend. Thanks for diving with us! For just $99,999, you can upgrade to VIP2 status and enjoy a complimentary deep exploration of the castle’s interior.

        Waypoint 4 (End):
        Initiating decompression procedure… get ready to ascend.
     */



    //整个游戏的任务开始点
    //start point of this game, initialize and start task
    private IEnumerator TaskStart() {
        // 等两秒
        yield return new WaitForSeconds(2f);

        waypointController.ShowWaypoint(waypoint1);
        GameObject g = GameObject.Find("HUD_IncomingCord");
        AudioSource TASK_D = g.GetComponent<AudioSource>();

        //AudioSource TASK_D = GameObject.Find("HUD_IncomingCord").GetComponent<AudioSource>();
        HUD_textMessage.ShowText("INCOMING COORDINATES RPF2K1\nWAYPOINT NAVIGATION SYSTEM: ACTIVE", TASK_D);


        yield return new WaitForSeconds(2f);

        //play task01 voice
        TASK_D = GameObject.Find("AUDIO_A2_Intro").GetComponent<AudioSource>(); //This is Tour Center, Welcome, David.You have successfully descended to the plan...
        TASK_D.Play();
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


    private IEnumerator UnloadScene(string sceneName) {
        // 异步卸载场景
        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(sceneName);

        // 等待卸载完成
        while (!unloadOp.isDone)
            yield return null;

        yield return null; // 可选：再等一帧以稳定状态
    }


    void onWaypoint1Reached() {
        waypointController.ShowWaypoint(null);
        HUD_textMessage.ShowText("WAYPOINT RPF2K1\nSTATUS: REACHED", null, () => {
            waypointController.ShowWaypoint(waypoint2);
            AudioSource TASK_D = GameObject.Find("HUD_IncomingCord").GetComponent<AudioSource>();
            HUD_textMessage.ShowText("INCOMING COORDINATES RPF4K2\nWAYPOINT NAVIGATION SYSTEM: ACTIVE", TASK_D, () => {
                TASK_D = GameObject.Find("AUDIO_A2_WP1").GetComponent<AudioSource>();
                TASK_D.Play();
            });
        });
    }

    void onWaypoint2Reached() {
        waypointController.ShowWaypoint(null);
        HUD_textMessage.ShowText("WAYPOINT RPF4K2\nSTATUS: REACHED", null, () => {
            waypointController.ShowWaypoint(waypoint3);
            AudioSource TASK_D = GameObject.Find("HUD_IncomingCord").GetComponent<AudioSource>();
            HUD_textMessage.ShowText("INCOMING COORDINATES RPF7K9\nWAYPOINT NAVIGATION SYSTEM: ACTIVE", TASK_D, () => {
                TASK_D = GameObject.Find("AUDIO_A2_WP2").GetComponent<AudioSource>();
                TASK_D.Play();
            });
        });
    }

    void onWaypoint3Reached() {
        waypointController.ShowWaypoint(null);
        HUD_textMessage.ShowText("WAYPOINT RPF7K9\nSTATUS: REACHED", null, () => {
            waypointController.ShowWaypoint(waypoint4);
            AudioSource TASK_D = GameObject.Find("HUD_IncomingCord").GetComponent<AudioSource>();
            HUD_textMessage.ShowText("INCOMING COORDINATES RPF4KE\nWAYPOINT NAVIGATION SYSTEM: ACTIVE", TASK_D, () => {
                TASK_D = GameObject.Find("AUDIO_A2_WP3").GetComponent<AudioSource>();
                TASK_D.Play();
            });
        });
    }

    void onWaypoint4Reached() {
        waypointController.ShowWaypoint(null);
        HUD_textMessage.ShowText("WAYPOINT RPF4KE\nSTATUS: REACHED", null);
        AudioSource TASK_D = GameObject.Find("AUDIO_A2_WP4").GetComponent<AudioSource>();
        TASK_D.Play();
        StartCoroutine(WaitForSeconds(TASK_D.clip.length + 2f, () => {
            StartCoroutine(UnloadScene("R_Area2 Under Water"));
        }));
    }
}
