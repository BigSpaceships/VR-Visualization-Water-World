using UnityEngine;

public class TRIGGER_1 : MonoBehaviour {
    private bool hasTriggered = false; // ��¼�Ƿ��Ѿ�����

    void OnTriggerEnter(Collider other) {
        if (!hasTriggered && other.CompareTag("Player")) //trigger only once
        {
            hasTriggered = true; // ����Ѵ���
            HUD_TextMessage textMessage = Object.FindFirstObjectByType<HUD_TextMessage>();
            textMessage.ShowText("Mission Started...\nProceed with Caution.", null, () => {
                Debug.Log("��Ļ������ɣ�");
            });
        }
    }
}
