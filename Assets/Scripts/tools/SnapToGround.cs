using UnityEngine;
using UnityEditor;

public class SnapToGround : Editor {
    [MenuItem("Tools/Snap Selected Objects To Ground %#g")] // Ctrl + Shift + G
    static void SnapSelectedObjects() {
        foreach (GameObject obj in Selection.gameObjects) {
            Collider col = obj.GetComponent<Collider>();
            if (col == null) {
                Debug.LogWarning($"对象 [{obj.name}] 没有 Collider，跳过。");
                continue;
            }

            // 获取物体底部中心点（世界坐标）
            Vector3 bottomCenter = new Vector3(
                col.bounds.center.x,
                col.bounds.min.y + 0.01f, // 微微抬起，避免射线打到自己
                col.bounds.center.z
            );

            // 从底部往下发射射线
            if (Physics.Raycast(bottomCenter, Vector3.down, out RaycastHit hit, 5f)) {
                float offset = col.bounds.min.y - obj.transform.position.y;

                Undo.RecordObject(obj.transform, "Snap To Ground");
                obj.transform.position = new Vector3(
                    obj.transform.position.x,
                    hit.point.y - offset,
                    obj.transform.position.z
                );
            } else {
                Debug.LogWarning($"对象 [{obj.name}] 底部未检测到地面，跳过。");
            }
        }
    }
}
