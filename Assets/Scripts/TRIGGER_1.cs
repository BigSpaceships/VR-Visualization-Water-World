using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TRIGGER_1 : MonoBehaviour {
    private bool hasTriggered = false; // ��¼�Ƿ��Ѿ�����

    void OnTriggerEnter(Collider other) {
        if (!hasTriggered && other.CompareTag("Player")) //trigger only once
        {
            hasTriggered = true; // ����Ѵ���
            HUD_TextMessage textMessage = Object.FindFirstObjectByType<HUD_TextMessage>();
            textMessage.ShowText("Mission Started...\nProceed with Caution.", null, () => {
                //Debug.Log("��Ļ������ɣ�");
            });

            StartCoroutine(SwitchScene());
        }
    }
    private IEnumerator SwitchScene() {
        // �����³���
        AsyncOperation loadOp = SceneManager.LoadSceneAsync("TestSwitchScene2", LoadSceneMode.Additive);
        while (!loadOp.isDone)
            yield return null;

        yield return null; // �ȴ�һ֡���ȶ�״̬

        // ж�ؾɳ���
        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync("TestSwitchScene");
        while (!unloadOp.isDone)
            yield return null;
    }
}
