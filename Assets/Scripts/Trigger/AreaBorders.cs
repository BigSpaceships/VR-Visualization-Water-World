using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AreaLoaderController : MonoBehaviour {
    public Transform playerTransform;
    public float defaultTriggerDistance = 40f;

    private enum AreaState {
        Unknown,   // ��ʼ��״̬
        Inside,    // ��������
        Outside    // ��������
    }

    private class AreaData {
        public Transform areaTransform;
        public List<Vector2> points = new();
        public AreaState insideState = AreaState.Unknown;
    }

    private List<AreaData> areas = new();

    void Start() {
        foreach (Transform area in transform) {
            AreaData areaData = new AreaData();
            areaData.areaTransform = area;

            foreach (Transform point in area) {
                Vector2 pos = new Vector2(point.position.x, point.position.z); // ʹ�� XZ ƽ��
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
                triggerDist = data.GetFloat("TriggerDistance", defaultTriggerDistance); // ��δ��������Ĭ��

            bool inside = IsPointInPolygon(playerPos, area.points); 
            float distance = DistanceToPolygon(playerPos, area.points);
            bool shouldBeInside = inside || distance < triggerDist;

            if (area.insideState!=AreaState.Inside && shouldBeInside) {
                area.insideState = AreaState.Inside;
                OnEnterArea(area.areaTransform);
            } else if (area.insideState != AreaState.Outside && !shouldBeInside) {
                area.insideState = AreaState.Outside;
                OnExitArea(area.areaTransform);
            }
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
            if (lowSceneName!=null) {
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

    private IEnumerator LoadScene(string sceneName) {
        var scene = SceneManager.GetSceneByName(sceneName);
        if (scene.isLoaded || SceneManager.GetSceneByName("Skiing").isLoaded) {
            yield break;
        }

        // �����³���
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!loadOp.isDone)
            yield return null;

        yield return null; // �ȴ�һ֡���ȶ�״̬
    }

    private IEnumerator UnloadScene(string sceneName) {
        // 1. ��鳡���Ƿ��� Build Settings ��
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.IsValid()) {
            //Debug.LogWarning($"[{nameof(UnloadScene)}] Scene ��{sceneName}�� is not in Build Settings.");
            yield break;
        }

        // 2. ��鳡���Ƿ�ǰ�Ѽ���
        if (!scene.isLoaded) {
            //Debug.LogWarning($"[{nameof(UnloadScene)}] Scene ��{sceneName}�� is not loaded.");
            yield break;
        }

        // 3. �첽ж��
        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(sceneName);
        if (unloadOp == null) {
            //Debug.LogError($"[{nameof(UnloadScene)}] Failed to start unloading scene ��{sceneName}��.");
            yield break;
        }

        // 4. �ȴ�ж�����
        while (!unloadOp.isDone)
            yield return null;

        // 5. ����ѡ���ٵ�һ֡��ȷ��״̬�ȶ�
        yield return null;
    }

    // ����㵽����αߵ��������
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
