using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TRIGGER_1 : MonoBehaviour {
    private bool hasTriggered = false; // 记录是否已经触发

    void OnTriggerEnter(Collider other) {
        if (!hasTriggered && other.CompareTag("Player")) //trigger only once
        {
            hasTriggered = true; // 标记已触发
            HUD_TextMessage textMessage = Object.FindFirstObjectByType<HUD_TextMessage>();
            textMessage.ShowText("Mission Started...\nProceed with Caution.", null, () => {
                //Debug.Log("字幕播放完成！");
            });

            StartCoroutine(SwitchScene());
        }
    }
    private IEnumerator SwitchScene() {
        // 加载新场景
        AsyncOperation loadOp = SceneManager.LoadSceneAsync("TestSwitchScene2", LoadSceneMode.Additive);
        while (!loadOp.isDone)
            yield return null;

        yield return null; // 等待一帧以稳定状态

        // 卸载旧场景
        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync("TestSwitchScene");
        while (!unloadOp.isDone)
            yield return null;
    }
}
