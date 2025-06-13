/*using UnityEngine;
using UnityEditor;

public class SnapToGround : Editor {
    [MenuItem("Tools/Snap Selected Objects To Ground %#g")] // Ctrl + Shift + G
    static void SnapSelectedObjects() {
        foreach (GameObject obj in Selection.gameObjects) {
            Collider col = obj.GetComponent<Collider>();
            if (col == null) {
                Debug.LogWarning($"���� [{obj.name}] û�� Collider��������");
                continue;
            }

            // ��ȡ����ײ����ĵ㣨�������꣩
            Vector3 bottomCenter = new Vector3(
                col.bounds.center.x,
                col.bounds.min.y + 0.01f, // ΢΢̧�𣬱������ߴ��Լ�
                col.bounds.center.z
            );

            // �ӵײ����·�������
            if (Physics.Raycast(bottomCenter, Vector3.down, out RaycastHit hit, 5f)) {
                float offset = col.bounds.min.y - obj.transform.position.y;

                Undo.RecordObject(obj.transform, "Snap To Ground");
                obj.transform.position = new Vector3(
                    obj.transform.position.x,
                    hit.point.y - offset,
                    obj.transform.position.z
                );
            } else {
                Debug.LogWarning($"���� [{obj.name}] �ײ�δ��⵽���棬������");
            }
        }
    }
}*/
