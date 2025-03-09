using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class HUD_TextMessage : MonoBehaviour {
    public TextMeshProUGUI textDisplay; // ��ʾ�ı��� TMP ���
    public AudioSource beepSound; // common char sound
    public AudioSource spaceSound; // space char sound
    public AudioSource voiceOver; // �������ţ���ѡ��
    public float letterDelay = 0.05f; // ÿ����ĸ�ļ��ʱ��
    public float lineDelay = 2f; // ÿ��֮��ļ��ʱ��
    public float clearDelay = 2f; // ��Ļ��ɺ�ȴ� 2 �����

    private Queue<string> textLines = new Queue<string>(); // �洢Ҫ���ŵ��ı�
    private bool isTyping = false; // ��ֹ�ظ�����
    private UnityAction onCompleteCallback; // ������ɺ�Ļص�

    public void Start() {
        textDisplay.text = "";
    }

    /// <summary>
    /// �ⲿ���ô˺��������� HUD ��ʾ��Ϣ
    /// </summary>
    /// <param name="text">Ҫ��ʾ���ı���֧�ֶ��У�</param>
    /// <param name="voice">Ҫ���ŵ����� `AudioSource`����Ϊ�գ�</param>
    /// <param name="callback">������ɺ�Ļص�����ѡ��</param>
    public void ShowText(string text, AudioSource voice = null, UnityAction callback = null) {
        if (isTyping) return; // ��ֹ�ظ�����
        onCompleteCallback = callback; // ��¼�ص�

        textDisplay.text = ""; // �����ʾ
        textLines.Clear(); // ��ն���

        // ���в���ı�
        string[] lines = text.Split('\n');
        foreach (string line in lines) {
            textLines.Enqueue(line);
        }

        // play voice
        if (voice != null) {
            voice.Play();
        }

        StartCoroutine(TypeText());
    }

    /// <summary>
    /// ������ʾ�ı���Э��
    /// </summary>
    private IEnumerator TypeText() {
        isTyping = true;
        while (textLines.Count > 0) {
            string line = textLines.Dequeue();
            textDisplay.text = ""; // ��յ�ǰ��

            foreach (char letter in line) {
                textDisplay.text += letter;

                // ���Ų�ͬ������
                if (letter == ' ') {
                    if (spaceSound != null) {
                        spaceSound.Play();
                    }
                } else {
                    if (beepSound != null) {
                        beepSound.Play();
                    }
                }

                yield return new WaitForSeconds(letterDelay);
            }

            // ÿ�в������ȴ�һ��ʱ��
            yield return new WaitForSeconds(lineDelay);
        }

        isTyping = false;

        // ����������󣬵ȴ� 2 ������ HUD
        yield return new WaitForSeconds(clearDelay);
        textDisplay.text = "";

        // �����ص�������У�
        onCompleteCallback?.Invoke();
    }
}

