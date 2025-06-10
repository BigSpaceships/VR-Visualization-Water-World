#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class SceneDebugDrawer : MonoBehaviour {
    // ��������������ǵ��ı�
    public List<TrackedDebugInfo> debugInfos = new List<TrackedDebugInfo>();

    [System.Serializable]
    public class TrackedDebugInfo {
        public Transform target;   // Ҫ��ʾ�ı�������
        public string text;        // ��ʾ����
        public Vector3 offset = new Vector3(0, 2, 0); // ƫ����
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
