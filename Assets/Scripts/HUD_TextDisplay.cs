﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HUD_TextDisplay : MonoBehaviour {
    public int baseDepth = 100;
    public float remainingOxygen = 400; // 剩余氧气量 (L)
    public float oxygenTankCapacity = 500.0f; // 氧气瓶总容量 (L)
    public AudioSource VOICE_Oxygen0 = null; //oxygen below 0
    public AudioSource VOICE_Oxygen30 = null; //oxygen below 30s
    public AudioSource VOICE_Oxygen60 = null; //oxygen below 60s
    public AudioSource VOICE_OxygenOnline = null; //oxygen online
    public TextMeshProUGUI depthText;
    public TextMeshProUGUI oxygenText;
    public TextMeshProUGUI pressureText;
    public TextMeshProUGUI timeRemainingText;
    public TextMeshProUGUI oxygenPercentageText; // 显示氧气剩余百分比

    private HUD_TextMessage HUD_textMessage;
    private float tankPressure = 200.0f; // 初始压力 (bar)
    private float depthPressureFactor = 1.0f; // 水深压力 (随深度增加)
    private float airConsumptionRate = 20.0f; // 标准消耗 (L/min, 参考普通潜水员)
    private bool oxygen0Played = false;  //play once
    private bool oxygen30Played = false; //play once
    private bool oxygen60Played = false; //play once

    void Start() {
        HUD_textMessage = Object.FindFirstObjectByType<HUD_TextMessage>();
    }

    void Update() {
        // 模拟氧气消耗
        if (oxygenText != null && pressureText != null && timeRemainingText != null) {
            // 计算水深压力 (每 10m 额外 +1 bar)
            float depth = -Camera.main.transform.position.y + baseDepth;
            depthPressureFactor = 1.0f + (depth / 100.0f); // 水深压力因子

            // 计算氧气消耗 (随深度增加)
            float oxygenUsedPerSecond = (airConsumptionRate / 60.0f) * depthPressureFactor;
            remainingOxygen = Mathf.Max(0, remainingOxygen - oxygenUsedPerSecond * Time.deltaTime);

            // 计算瓶内压力 (bar)
            tankPressure = (remainingOxygen / oxygenTankCapacity) * 200.0f;

            // 计算剩余时间 (分钟)
            float estimatedTimeLeft = remainingOxygen / (airConsumptionRate * depthPressureFactor);

            // 计算剩余氧气百分比
            float oxygenPercentage = (remainingOxygen / oxygenTankCapacity) * 100.0f;

            // 更新 UI 显示
            pressureText.text = $"Tank Pressure: {tankPressure.ToString("F1")} bar";
            oxygenText.text = $"Oxygen: {remainingOxygen.ToString("F1")}L / {oxygenTankCapacity.ToString("F1")}L";
            oxygenPercentageText.text = $"Oxygen Remaining: {oxygenPercentage.ToString("F1")}%";
            timeRemainingText.text = estimatedTimeLeft > 0
                ? $"Estimated Time Left: {estimatedTimeLeft.ToString("F1")} min"
                : "<color=red>Oxygen Depleted!</color>";

            if (estimatedTimeLeft <= 1f && !oxygen60Played) {
                HUD_textMessage.ShowText("WARNING: OXYGEN CRITICAL\n 60 SECONDS REMAINING", VOICE_Oxygen60);
                oxygen60Played = true;
            }

            if (estimatedTimeLeft <= 0.5f && !oxygen30Played) {
                HUD_textMessage.ShowText("WARNING: OXYGEN CRITICAL\n 30 SECONDS REMAINING", VOICE_Oxygen30);
                oxygen30Played = true;
            }

            // 🚨 播放 **"氧气已耗尽"** 语音（氧气为 0）
            if (remainingOxygen <= 0 && !oxygen0Played) {
                oxygen0Played = true;
                HUD_textMessage.ShowText("⚠ WARNING: OXYGEN DEPLETED.\nCURRENT TANK OFFLINE.", VOICE_Oxygen0, () => {
                    // ✅ 3 秒后显示下一条 HUD 信息 + 播放 TASK_D03
                    StartCoroutine(WaitForSeconds(3f, () => {
                        oxygenTankCapacity = 900;
                        remainingOxygen = 900;
                        estimatedTimeLeft = remainingOxygen / (airConsumptionRate * depthPressureFactor);
                        oxygen0Played = false;
                        HUD_textMessage.ShowText($"PRIMARY OXYGEN TANK ONLINE. PRESSURE STABILIZING.\nESTIMATED OXYGEN REMAINING: {estimatedTimeLeft.ToString("F1")} MINUTES.", VOICE_OxygenOnline);
                    }));
                });
            }
        }

        // 计算深度
        if (depthText != null) {
            float depth = -Camera.main.transform.position.y + baseDepth;
            // 计算水压：水面 1 bar，每 10m 增加 1 bar
            float pressure = 1.0f + Mathf.Abs(depth) / 10.0f;

            // 更新 UI 显示水深和压力
            depthText.text = $"Depth: {Mathf.Abs(depth).ToString("F1")}m\nPressure: {pressure.ToString("F2")} bar";
        }
    }

    IEnumerator WaitForSeconds(float time, System.Action callback) {
        yield return new WaitForSeconds(time);
        callback?.Invoke();
    }
}
