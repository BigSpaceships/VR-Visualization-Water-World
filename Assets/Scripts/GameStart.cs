﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.HID;

public class GameStart : MonoBehaviour {
    public Transform waypoint1;
    public Transform waypoint2;
    public Transform waypoint3;
    public Transform waypoint4;
    public Transform player;
    public float waypointActiveRadius = 3.0f;  //how may meters to reach(touch) waypoint

    private HUD_TextMessage HUD_textMessage;
    private HUD_WayPoint waypointController;

    // Start is called before the first frame update
    void Start() {
        HUD_textMessage = Object.FindFirstObjectByType<HUD_TextMessage>();
        waypointController = Object.FindFirstObjectByType<HUD_WayPoint>();
        waypointController.ShowWaypoint(null); //clear waypoint

        //play task01 voice
        AudioSource TASK_D01 = GameObject.Find("TASK_D01").GetComponent<AudioSource>();
        TASK_D01.Play();
        StartCoroutine(WaitForSeconds(TASK_D01.clip.length, () => {
            Transform WayPoint1 = GameObject.Find("WayPoint1")?.transform;
            waypointController.ShowWaypoint(WayPoint1);
            HUD_textMessage.ShowText("INCOMING COORDINATES\nWAYPOINT NAVIGATION SYSTEM\nSTATUS: ACTIVE", null);

        }));
        //Invoke("ShowWelcomeText", 3f); // 3秒后调用 `ShowWelcomeText`
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
        HUD_textMessage.ShowText("WAYPOINT A\nSTATUS: REACHED", null);
        waypointController.ShowWaypoint(waypoint2);
    }

    void onWaypoint2Reached() {
        HUD_textMessage.ShowText("WAYPOINT B\nSTATUS: REACHED", null);
        waypointController.ShowWaypoint(waypoint3);
    }

    void onWaypoint3Reached() {
        HUD_textMessage.ShowText("WAYPOINT C\nSTATUS: REACHED", null);
        waypointController.ShowWaypoint(waypoint4);
    }

    void onWaypoint4Reached() {
        HUD_textMessage.ShowText("WAYPOINT D\nSTATUS: REACHED", null);
    }
}
