using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TriggerArea2 : MonoBehaviour {
    private bool hasTriggered = false; // 是否已经触发过

    private void OnTriggerEnter(Collider other) {
        //Debug.Log("trigger step 1");
        if (other.CompareTag("Player") && !hasTriggered) {
            hasTriggered = true;
            //Debug.Log("triggered");
            StartCoroutine(SwitchScene());
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            hasTriggered = false;
            //Debug.Log("🔄 离开空气墙：状态已复位。");
        }
    }

    private IEnumerator SwitchScene() {
        // 加载新场景
        AsyncOperation loadOp = SceneManager.LoadSceneAsync("R_Area2 Under Water", LoadSceneMode.Additive);
        while (!loadOp.isDone)
            yield return null;

        yield return null; // 等待一帧以稳定状态

        // 卸载旧场景Area0主场景不删除
        //AsyncOperation unloadOp = SceneManager.UnloadSceneAsync("TestSwitchScene");
        //while (!unloadOp.isDone)
        //    yield return null;
    }
}
