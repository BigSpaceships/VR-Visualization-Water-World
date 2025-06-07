using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AreaLoaderController : MonoBehaviour {
    public Transform playerTransform;
    public float defaultTriggerDistance = 40f;

    private class AreaData {
        public Transform areaTransform;
        public List<Vector2> points = new();
        public bool isInside = false;
    }

    private List<AreaData> areas = new();

    void Start() {
        foreach (Transform area in transform) {
            AreaData areaData = new AreaData();
            areaData.areaTransform = area;

            foreach (Transform point in area) {
                Vector2 pos = new Vector2(point.position.x, point.position.z); // 使用 XZ 平面
                areaData.points.Add(pos);
            }

            areas.Add(areaData);
        }
    }

    void Update() {
        Vector2 playerPos = new Vector2(playerTransform.position.x, playerTransform.position.z);

        foreach (var area in areas) {
            float triggerDist = defaultTriggerDistance;
            ObjectData data = area.areaTransform.GetComponent<ObjectData>();
            if (data != null)
                triggerDist = data.GetFloat("TriggerDistance", defaultTriggerDistance); // 若未设置则用默认

            bool inside = IsPointInPolygon(playerPos, area.points); 
            float distance = DistanceToPolygon(playerPos, area.points);
            bool shouldBeInside = inside || distance < triggerDist;

            if (!area.isInside && shouldBeInside) {
                area.isInside = true;
                OnEnterArea(area.areaTransform);
            } else if (area.isInside && !shouldBeInside) {
                area.isInside = false;
                OnExitArea(area.areaTransform);
            }
        }
    }

    void OnEnterArea(Transform area) {
        ObjectData data = area.GetComponent<ObjectData>();
        if (data != null) {
            string sceneName = data.Get("SceneName");
            if (sceneName != null) {
                StartCoroutine(LoadScene(sceneName));
            }
        }
    }

    void OnExitArea(Transform area) {
        ObjectData data = area.GetComponent<ObjectData>();
        if (data != null) {
            string sceneName = data.Get("SceneName");
            if (!string.IsNullOrEmpty(sceneName)) {
                StartCoroutine(UnloadScene(sceneName));
            }
        }
    }

    bool IsPointInPolygon(Vector2 point, List<Vector2> polygon) {
        bool inside = false;
        int j = polygon.Count - 1;

        for (int i = 0; i < polygon.Count; i++) {
            if ((polygon[i].y > point.y) != (polygon[j].y > point.y) &&
                point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) /
                          (polygon[j].y - polygon[i].y) + polygon[i].x) {
                inside = !inside;
            }
            j = i;
        }

        return inside;
    }

    private IEnumerator LoadScene(string sceneName) {
        // 加载新场景
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!loadOp.isDone)
            yield return null;

        yield return null; // 等待一帧以稳定状态
    }

    private IEnumerator UnloadScene(string sceneName) {
        // 异步卸载场景
        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(sceneName);

        // 等待卸载完成
        while (!unloadOp.isDone)
            yield return null;

        yield return null; // 可选：再等一帧以稳定状态
    }

    // 计算点到多边形边的最近距离
    float DistanceToPolygon(Vector2 point, List<Vector2> polygon) {
        float minDist = float.MaxValue;
        for (int i = 0; i < polygon.Count; i++) {
            Vector2 a = polygon[i];
            Vector2 b = polygon[(i + 1) % polygon.Count];
            float d = DistancePointToSegment(point, a, b);
            if (d < minDist)
                minDist = d;
        }
        return minDist;
    }

    float DistancePointToSegment(Vector2 p, Vector2 a, Vector2 b) {
        Vector2 ab = b - a;
        Vector2 ap = p - a;
        float t = Vector2.Dot(ap, ab) / ab.sqrMagnitude;
        t = Mathf.Clamp01(t);
        Vector2 closest = a + t * ab;
        return Vector2.Distance(p, closest);
    }
}
