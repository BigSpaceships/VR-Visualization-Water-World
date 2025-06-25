using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

public class AreaLoaderController : MonoBehaviour {
    public Transform playerTransform;
    public float defaultTriggerDistance = 40f;
    public bool Enabled = true;

    private enum AreaState {
        Unknown,   // 初始化状态
        Inside,    // 在区域内
        Outside    // 在区域外
    }

    private class AreaData {
        public Transform areaTransform;
        public List<Vector2> points = new();
        public AreaState insideState = AreaState.Unknown;
    }

    private List<AreaData> areas = new();
    private bool isRefreshing = false;

    void Start() {
        ResetData();
    }

    public void ResetData() {
        areas.Clear();
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

    //unload all scenes and turn to disabled status
    public IEnumerator UnloadAllScenesCoroutine() {
        Enabled = false;
        List<Scene> loadedScenes = new List<Scene>();
        for (int i = 0; i < SceneManager.sceneCount; i++)
            loadedScenes.Add(SceneManager.GetSceneAt(i));

        foreach (var scene in loadedScenes) {
            yield return StartCoroutine(UnloadScene(scene.name));
        }
        ResetData();
    }

    public IEnumerator RefreshCoroutine() {
        isRefreshing = true;
        Vector2 playerPos = new Vector2(playerTransform.position.x, playerTransform.position.z);

        foreach (var area in areas) {
            try {
                float triggerDist = defaultTriggerDistance;
                ObjectData data = area.areaTransform.GetComponent<ObjectData>();
                if (data != null)
                    triggerDist = data.GetFloat("TriggerDistance", defaultTriggerDistance); // 若未设置则用默认

                bool inside = IsPointInPolygon(playerPos, area.points);
                float distance = DistanceToPolygon(playerPos, area.points);
                bool shouldBeInside = inside || distance < triggerDist;

                if (area.insideState != AreaState.Inside && shouldBeInside) {
                    area.insideState = AreaState.Inside;
                    OnEnterArea(area.areaTransform);
                } else if (area.insideState != AreaState.Outside && !shouldBeInside) {
                    area.insideState = AreaState.Outside;
                    OnExitArea(area.areaTransform);
                }
            } catch (System.Exception ex) {
                Debug.LogException(ex);
            }
            yield return null;
        }
        isRefreshing = false;
        Enabled = true;
        yield break;
    }

    void Update() {
        if (!Enabled) return;
        if (!isRefreshing) {
            StartCoroutine(RefreshCoroutine());
        }
    }

    void OnEnterArea(Transform area) {
        ObjectData data = area.GetComponent<ObjectData>();
        if (data != null) {
            string sceneName = data.Get("SceneName");
            string lowSceneName = data.Get("LowSceneName");
            if (sceneName != null) {
                StartCoroutine(LoadScene(sceneName));
            }
            if (lowSceneName != null) {
                StartCoroutine(UnloadScene(lowSceneName));
            }
        }
    }

    void OnExitArea(Transform area) {
        ObjectData data = area.GetComponent<ObjectData>();
        if (data != null) {
            string sceneName = data.Get("SceneName");
            string lowSceneName = data.Get("LowSceneName");
            if (!string.IsNullOrEmpty(sceneName)) {
                StartCoroutine(UnloadScene(sceneName));
            }
            if (lowSceneName != null) {
                StartCoroutine(LoadScene(lowSceneName));
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

    public IEnumerator LoadScene(string sceneName) {
        var scene = SceneManager.GetSceneByName(sceneName);
        if (scene.isLoaded) {
            yield break;
        }

        // 加载新场景
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!loadOp.isDone)
            yield return null;

        yield return null; // 等待一帧以稳定状态
    }

    public IEnumerator UnloadScene(string sceneName) {
        // 1. 检查场景是否在 Build Settings 里
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.IsValid()) {
            Debug.LogWarning($"[{nameof(UnloadScene)}] Scene “{sceneName}” is not in Build Settings.");
            yield break;
        }

        // 2. 检查场景是否当前已加载
        if (!scene.isLoaded) {
            Debug.LogWarning($"[{nameof(UnloadScene)}] Scene “{sceneName}” is not loaded.");
            yield break;
        }

        // 3. 异步卸载
        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(sceneName);
        if (unloadOp == null) {
            Debug.LogError($"[{nameof(UnloadScene)}] Failed to start unloading scene “{sceneName}”.");
            yield break;
        }

        // 4. 等待卸载完成
        while (!unloadOp.isDone)
            yield return null;

        // 5. （可选）再等一帧以确保状态稳定
        yield return null;
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
