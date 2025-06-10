#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[InitializeOnLoad]
public static class DebugText3D {
    class DebugTextItem {
        public Transform target;
        public string message;
        public float endTime;
        public Vector3 offset;
        public Color color;
    }

    static readonly List<DebugTextItem> activeItems = new();

    static DebugText3D() {
        SceneView.duringSceneGui += OnSceneGUI;
        EditorApplication.update += Cleanup;
    }

    public static void Show(Transform target, string message, float duration = 1.5f, float height = 2f, Color? color = null) {
        if (target == null) return;

        activeItems.Add(new DebugTextItem {
            target = target,
            message = message,
            endTime = Time.realtimeSinceStartup + duration,
            offset = new Vector3(0, height, 0),
            color = color ?? Color.yellow
        });
    }

    private static void OnSceneGUI(SceneView view) {
        Handles.BeginGUI();

        foreach (var item in activeItems) {
            if (item == null || item.target == null) continue;

            Vector3 worldPos = item.target.position + item.offset;
            Vector3 screenPos = HandleUtility.WorldToGUIPoint(worldPos);

            // 可选：视距裁剪（隐藏太远的文字）
            float camDistance = Vector3.Distance(SceneView.lastActiveSceneView.camera.transform.position, worldPos);
            if (camDistance > 50f) continue; // 超过 50 米不显示

            GUIStyle style = new GUIStyle(EditorStyles.boldLabel) {
                normal = { textColor = item.color },
                alignment = TextAnchor.UpperCenter,
                fontSize = 12
            };

            Rect labelRect = new Rect(screenPos.x - 50, screenPos.y - 20, 100, 40);
            GUI.Label(labelRect, item.message, style);
        }

        Handles.EndGUI();
    }

    private static void Cleanup() {
        float now = Time.realtimeSinceStartup;
        activeItems.RemoveAll(i => i.endTime < now || i.target == null);
    }
}
#endif
