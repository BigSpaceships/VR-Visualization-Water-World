using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class HUD_TextMessage : MonoBehaviour {
    public TextMeshProUGUI textDisplay; // 显示文本的 TMP 组件
    public AudioSource beepSound; // common char sound
    public AudioSource spaceSound; // space char sound
    public AudioSource voiceOver; // 语音播放（可选）
    public float letterDelay = 0.05f; // 每个字母的间隔时间
    public float lineDelay = 2f; // 每行之间的间隔时间
    public float clearDelay = 2f; // 字幕完成后等待 2 秒清空

    private Queue<string> textLines = new Queue<string>(); // 存储要播放的文本
    private bool isTyping = false; // 防止重复调用
    private UnityAction onCompleteCallback; // 播放完成后的回调

    public void Start() {
        textDisplay.text = "";
    }

    /// <summary>
    /// 外部调用此函数，播放 HUD 提示信息
    /// </summary>
    /// <param name="text">要显示的文本（支持多行）</param>
    /// <param name="voice">要播放的语音 `AudioSource`（可为空）</param>
    /// <param name="callback">播放完成后的回调（可选）</param>
    public void ShowText(string text, AudioSource voice = null, UnityAction callback = null) {
        if (isTyping) return; // 防止重复调用
        onCompleteCallback = callback; // 记录回调

        textDisplay.text = ""; // 清空显示
        textLines.Clear(); // 清空队列

        // 按行拆分文本
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
    /// 逐字显示文本的协程
    /// </summary>
    private IEnumerator TypeText() {
        isTyping = true;
        while (textLines.Count > 0) {
            string line = textLines.Dequeue();
            textDisplay.text = ""; // 清空当前行

            foreach (char letter in line) {
                textDisplay.text += letter;

                // 播放不同的声音
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

            // 每行播放完后等待一段时间
            yield return new WaitForSeconds(lineDelay);
        }

        isTyping = false;

        // 语音播放完后，等待 2 秒后清空 HUD
        yield return new WaitForSeconds(clearDelay);
        textDisplay.text = "";

        // 触发回调（如果有）
        onCompleteCallback?.Invoke();
    }
}

