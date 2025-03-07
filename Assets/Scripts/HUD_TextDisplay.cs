using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HUD_TextDisplay : MonoBehaviour {
    public int baseDepth = 100;
    public float remainingOxygen = 400; // ʣ�������� (L)
    public float oxygenTankCapacity = 500.0f; // ����ƿ������ (L)
    public TextMeshProUGUI depthText;
    public TextMeshProUGUI oxygenText;
    public TextMeshProUGUI pressureText;
    public TextMeshProUGUI timeRemainingText;
    public TextMeshProUGUI oxygenPercentageText; // ��ʾ����ʣ��ٷֱ�

    private float tankPressure = 200.0f; // ��ʼѹ�� (bar)
    private float depthPressureFactor = 1.0f; // ˮ��ѹ�� (���������)
    private float airConsumptionRate = 20.0f; // ��׼���� (L/min, �ο���ͨǱˮԱ)

    void Update() {
        // ģ����������
        if (oxygenText != null && pressureText != null && timeRemainingText != null) {
            // ����ˮ��ѹ�� (ÿ 10m ���� +1 bar)
            float depth = -Camera.main.transform.position.y + baseDepth;
            depthPressureFactor = 1.0f + (depth / 100.0f); // ˮ��ѹ������

            // ������������ (���������)
            float oxygenUsedPerSecond = (airConsumptionRate / 60.0f) * depthPressureFactor;
            remainingOxygen = Mathf.Max(0, remainingOxygen - oxygenUsedPerSecond * Time.deltaTime);

            // ����ƿ��ѹ�� (bar)
            tankPressure = (remainingOxygen / oxygenTankCapacity) * 200.0f;

            // ����ʣ��ʱ�� (����)
            float estimatedTimeLeft = remainingOxygen / (airConsumptionRate * depthPressureFactor);

            // ����ʣ�������ٷֱ�
            float oxygenPercentage = (remainingOxygen / oxygenTankCapacity) * 100.0f;

            // ���� UI ��ʾ
            pressureText.text = $"Tank Pressure: {tankPressure.ToString("F1")} bar";
            oxygenText.text = $"Oxygen: {remainingOxygen.ToString("F1")}L / {oxygenTankCapacity.ToString("F1")}L";
            oxygenPercentageText.text = $"Oxygen Remaining: {oxygenPercentage.ToString("F1")}%";
            timeRemainingText.text = estimatedTimeLeft > 0
                ? $"Estimated Time Left: {estimatedTimeLeft.ToString("F1")} min"
                : "<color=red>Oxygen Depleted!</color>";
        }

        // �������
        if (depthText != null) {
            float depth = -Camera.main.transform.position.y + baseDepth;
            // ����ˮѹ��ˮ�� 1 bar��ÿ 10m ���� 1 bar
            float pressure = 1.0f + Mathf.Abs(depth) / 10.0f;

            // ���� UI ��ʾˮ���ѹ��
            depthText.text = $"Depth: {Mathf.Abs(depth).ToString("F1")}m\nPressure: {pressure.ToString("F2")} bar";
        }
    }
}
