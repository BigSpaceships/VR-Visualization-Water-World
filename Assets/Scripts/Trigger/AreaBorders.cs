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
        while (isRefreshing)
            yield return null;
        Enabled = false;
        
        Debug.Log("start UnloadAllScenesCoroutine");
        List<Scene> loadedScenes = new List<Scene>();
        for (int i = 0; i < SceneManager.sceneCount; i++)
            loadedScenes.Add(SceneManager.GetSceneAt(i));

        foreach (var scene in loadedScenes) {
            if (scene.name == "R_Main") continue;
            yield return UnloadScene(scene.name);
            yield return null;
        }
        ResetData();
        yield break;
    }

    public IEnumerator RefreshCoroutine() {
        isRefreshing = true;
        Vector2 playerPos = new Vector2(playerTransform.position.x, playerTransform.position.z);

        foreach (var area in areas) {
            float triggerDist = defaultTriggerDistance;
            ObjectData data = area.areaTransform.GetComponent<ObjectData>();
            if (data != null)
                triggerDist = data.GetFloat("TriggerDistance", defaultTriggerDistance); // 若未设置则用默认

            bool inside = IsPointInPolygon(playerPos, area.points);
            float distance = DistanceToPolygon(playerPos, area.points);
            bool shouldBeInside = inside || distance < triggerDist;

            if (area.insideState != AreaState.Inside && shouldBeInside) {
                area.insideState = AreaState.Inside;
                yield return OnEnterArea(area.areaTransform);
            } else if (area.insideState != AreaState.Outside && !shouldBeInside) {
                area.insideState = AreaState.Outside;
                yield return OnExitArea(area.areaTransform);
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

    IEnumerator OnEnterArea(Transform area) {
        ObjectData data = area.GetComponent<ObjectData>();
        if (data != null) {
            string sceneName = data.Get("SceneName");
            string lowSceneName = data.Get("LowSceneName");
            yield return SwitchArea(sceneName, lowSceneName);
        }
    }

    IEnumerator OnExitArea(Transform area) {
        ObjectData data = area.GetComponent<ObjectData>();
        if (data != null) {
            string sceneName = data.Get("SceneName");
            string lowSceneName = data.Get("LowSceneName");
            yield return SwitchArea(lowSceneName, sceneName);
        }
    }

    public IEnumerator SwitchArea(string newArea, string oldArea) {
        if (!string.IsNullOrWhiteSpace(newArea)) {
            yield return LoadScene(newArea);
        }
        if (!string.IsNullOrWhiteSpace(oldArea)) {
            var main = SceneManager.GetSceneByName("R_Main");
            if (main.IsValid())
                SceneManager.SetActiveScene(main);
            else
                Debug.LogWarning("SwitchArea: 找不到 R_Main 场景，无法设为 Active Scene。");
            yield return UnloadScene(oldArea);
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

    /// <summary>
    /// 以 Additive 模式加载场景，加载完成后自动切成 Active Scene
    /// </summary>
    public IEnumerator LoadScene(string sceneName) {
        Debug.Log($"start load scene: {sceneName}");
        // 1) 如果已经加载，直接返回
        var scene = SceneManager.GetSceneByName(sceneName);
        if (scene.isLoaded)
            yield break;

        // 2) 异步加载
        var loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        if (loadOp == null) {
            Debug.LogError($"LoadScene: 无法开始加载场景 “{sceneName}”");
            yield break;
        }

        // 3) 等待加载完成
        yield return loadOp;
        Debug.Log($"load scene complete: {sceneName}");

        // 4) （可选）将新加载的场景设为 Active Scene
        //SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
    }


    /// <summary>
    /// 卸载指定场景，直接传 Scene 对象避免 “not in Build Settings” 警告
    /// </summary>
    public IEnumerator UnloadScene(string sceneName) {
        Debug.Log($"start unload scene: {sceneName}");
        var scene = SceneManager.GetSceneByName(sceneName);

        SceneManager.SetActiveScene(SceneManager.GetSceneByName("R_Main"));

        // 1) 场景有效且已加载
        if (!scene.IsValid() || !scene.isLoaded) {
            Debug.LogWarning($"UnloadScene: 场景 “{sceneName}” 未加载或无效，跳过卸载");
            yield break;
        }
        
        foreach (var go in scene.GetRootGameObjects())
            go.SetActive(false);       // 先把它们从渲染管线里剔除
        yield return null;            // 等一帧，让管线真的不再采样它们

        // 2) 调用基于 Scene 对象的重载
        var unloadOp = SceneManager.UnloadSceneAsync(scene);
        if (unloadOp == null) {
            Debug.LogError($"UnloadScene: 无法开始卸载场景 “{sceneName}”");
            yield break;
        }

        // 3) 等待卸载完成
        yield return unloadOp;
        Debug.Log($"unload scene complete: {sceneName}");
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
