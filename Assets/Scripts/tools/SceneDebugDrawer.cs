#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class SceneDebugDrawer : MonoBehaviour {
    // 管理多个物体和它们的文本
    public List<TrackedDebugInfo> debugInfos = new List<TrackedDebugInfo>();

    [System.Serializable]
    public class TrackedDebugInfo {
        public Transform target;   // 要显示文本的物体
        public string text;        // 显示内容
        public Vector3 offset = new Vector3(0, 2, 0); // 偏移量
    }

    void OnDrawGizmos() {
        foreach (var info in debugInfos) {
            if (info.target == null) continue;

            Vector3 pos = info.target.position + info.offset;
            Handles.Label(pos, info.text);
            Debug.DrawLine(info.target.position, pos, Color.yellow);
        }
    }
}
#endif
