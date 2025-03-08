using UnityEngine;

public class TRIGGER_1 : MonoBehaviour {
    private bool hasTriggered = false; // 记录是否已经触发

    void OnTriggerEnter(Collider other) {
        if (!hasTriggered && other.CompareTag("Player")) //trigger only once
        {
            hasTriggered = true; // 标记已触发
            HUD_TextMessage textMessage = Object.FindFirstObjectByType<HUD_TextMessage>();
            textMessage.ShowText("Mission Started...\nProceed with Caution.", null, () => {
                Debug.Log("字幕播放完成！");
            });
        }
    }
}
