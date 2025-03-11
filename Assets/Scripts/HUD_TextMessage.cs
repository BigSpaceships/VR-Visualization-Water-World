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
    private Queue<(string, AudioSource, UnityAction)> messageQueue = new Queue<(string, AudioSource, UnityAction)>(); // 📌 消息队列

    public void Start() {
        textDisplay.text = "";
    }

    public void ShowText(string text, AudioSource voice = null, UnityAction callback = null) {
        messageQueue.Enqueue((text, voice, callback)); // ✅ 将文本加入队列

        if (!isTyping) {
            StartCoroutine(ProcessQueue()); // ✅ 如果当前没在播放，开始播放队列
        }
    }

    private IEnumerator ProcessQueue() {
        while (messageQueue.Count > 0) // ✅ 队列中有消息就播放
        {
            isTyping = true;

            var (text, voice, callback) = messageQueue.Dequeue(); // 取出最早的一条信息

            textDisplay.text = ""; // 清空 HUD
            string[] lines = text.Split('\n');
            Queue<string> textLines = new Queue<string>(lines);

            // 播放语音（如果有）
            if (voice != null) voice.Play();

            while (textLines.Count > 0) {
                string line = textLines.Dequeue();
                textDisplay.text = "";

                foreach (char letter in line) {
                    textDisplay.text += letter;

                    // 处理不同的声音
                    if (letter == ' ') {
                        spaceSound?.Play();
                    } else {
                        beepSound?.Play();
                    }

                    yield return new WaitForSeconds(letterDelay);
                }

                yield return new WaitForSeconds(lineDelay);
            }

            yield return new WaitForSeconds(clearDelay); // ✅ 等待清除时间
            textDisplay.text = ""; // 清空 HUD

            callback?.Invoke(); // ✅ 调用回调（如果有）
        }

        isTyping = false; // ✅ 播放完毕，允许新的 `ShowText()` 启动
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

