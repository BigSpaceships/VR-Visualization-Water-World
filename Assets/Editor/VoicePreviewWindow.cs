#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

public class VoicePreviewWindow : EditorWindow {
    AudioClip clip;
    AudioMixerGroup mixerGroup;

    GameObject previewGO;
    AudioSource previewSrc;
    double startTime;

    [MenuItem("Window/Voice Preview")]
    static void Open() => GetWindow<VoicePreviewWindow>("Voice Preview");

    void OnGUI() {
        clip = (AudioClip)EditorGUILayout.ObjectField("Clip", clip, typeof(AudioClip), false);
        mixerGroup = (AudioMixerGroup)EditorGUILayout.ObjectField("Mixer Group", mixerGroup, typeof(AudioMixerGroup), false);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Play Preview") && clip != null && mixerGroup != null) {
            StartPreview();
        }
        if (GUILayout.Button("Stop Preview")) {
            CleanupPreview();
        }
        GUILayout.EndHorizontal();
    }

    void StartPreview() {
        CleanupPreview();

        previewGO = new GameObject("VoicePreview");
        previewSrc = previewGO.AddComponent<AudioSource>();
        previewSrc.clip = clip;
        previewSrc.outputAudioMixerGroup = mixerGroup;
        previewSrc.playOnAwake = false;
        previewSrc.Play();

        startTime = EditorApplication.timeSinceStartup;
        EditorApplication.update += OnEditorUpdate;
    }

    void OnEditorUpdate() {
        if (clip != null && EditorApplication.timeSinceStartup - startTime > clip.length) {
            CleanupPreview();
        }
    }

    void CleanupPreview() {
        if (previewGO != null) {
            DestroyImmediate(previewGO);
            previewGO = null;
        }
        EditorApplication.update -= OnEditorUpdate;
    }

    void OnDisable() {
        CleanupPreview();
    }
}
#endif
