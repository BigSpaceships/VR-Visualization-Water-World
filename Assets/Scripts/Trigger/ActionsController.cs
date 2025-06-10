using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionsController : MonoBehaviour {
    public float turnSpeed = 200f; // 每秒旋转角度
    public Dictionary<string, int> animMap;  // 动画映射表，如 Walk -> 1

    private Transform animatedObject;         // 被控制的动画物体
    private Animation animationObj;           
    private ObjectData _objectData;

    private class CachedAction {
        public Vector3 worldPosition;
        public Quaternion worldRotation;
        public ObjectData data;
    }
    private List<CachedAction> cachedActions = new List<CachedAction>();

    private void Awake() {
        animatedObject = transform.parent;
        animationObj = animatedObject.GetComponent<Animation>();
        CacheAllActions();

        // 拿到挂在同一物体上的 ObjectData（动画配置表）
        _objectData = GetComponent<ObjectData>();
    }

    private void Start() {
        StartCoroutine(ExecuteActions());
    }

    private void CacheAllActions() {
        cachedActions.Clear();

        foreach (Transform child in transform)  // transform 是 Actions
        {
            ObjectData od = child.GetComponent<ObjectData>();
            if (od != null) {
                cachedActions.Add(new CachedAction {
                    worldPosition = child.position,  // 注意：存的是位置副本
                    worldRotation = child.rotation,
                    data = od
                });
            }
        }
    }

    IEnumerator ExecuteActions() {
        while (true) {
            foreach (CachedAction action in cachedActions) {
                ObjectData data = action.data;
                if (data == null) continue;

                if (data.data.Count <= 0) continue;
                string key = data.data[0].key;
                string value = data.data[0].value;
                var param = PubFunc.ParseParams(value);

                if (key == "Walk") {
                    //aniName=Armature|run1;speed=2;
                    // 1.计算目标位置
                    Vector3 targetPos = action.worldPosition;

                    // 播放跑步动画
                    animationObj.wrapMode = WrapMode.Loop;
                    animationObj.Play(param["aniName"]);

                    // 先朝向目标转身
                    Vector3 dir = (targetPos - animatedObject.position).normalized;
                    if (dir.sqrMagnitude > 0.001f) {
                        Quaternion targetRot = Quaternion.LookRotation(dir);
                        yield return StartCoroutine(RotateToRotation(targetRot, turnSpeed));
                    }

                    //移动过去
                    float speed = param.TryGetValue("speed", out var s) && float.TryParse(s, out var spd)  ? spd : 2f;
                    yield return StartCoroutine(MoveToPosition(targetPos, speed));

                    //停止跑步动画
                    animationObj.Stop(param["aniName"]);
                    AnimationState st = animationObj[param["aniName"]];
                    st.enabled = true;
                    st.time = 0f;
                    animationObj.Sample();
                    st.enabled = false;
                } else if (key == "Teleport") {
                    animatedObject.position = action.worldPosition;
                } else if (key == "Turn") {
                    //aniName=Armature|run1;
                    // 播放跑步动画
                    animationObj.wrapMode = WrapMode.Loop;
                    animationObj.Play(param["aniName"]);

                    // 先朝向目标转身
                    Debug.Log($"target:{action.worldRotation}, this: {animatedObject.rotation}");
                    yield return StartCoroutine(RotateToRotation(action.worldRotation, turnSpeed));

                    //停止跑步动画
                    animationObj.Stop(param["aniName"]);
                    AnimationState st = animationObj[param["aniName"]];
                    st.enabled = true;
                    st.time = 0f;
                    animationObj.Sample();
                    st.enabled = false;
                } else if (key == "Wait") {
                    //seconds=2;
                    yield return new WaitForSeconds(float.Parse(param["seconds"]));
                } else if (key == "PlayAni") {
                    //aniName=xArmature|ear1;playCount=1;
                    int playCount = 1;
                    if (param.TryGetValue("playCount", out var pt) && int.TryParse(pt, out var ptv) && ptv > 0)
                        playCount = ptv;

                    AnimationState st = animationObj[param["aniName"]];
                    if (st == null) {
                        Debug.LogWarning($"PlayAni 未找到动画剪辑: {param["aniName"]}");
                        continue;
                    }

                    // 单次播放模式
                    st.wrapMode = WrapMode.Once;
                    float clipLength = st.length;

                    // 循环播放
                    for (int i = 0; i < playCount; i++) {
                        animationObj.Play(param["aniName"]);
                        yield return new WaitForSeconds(clipLength);
                    }

                    // 播放完毕后复位到首帧
                    st.enabled = true;
                    st.time = 0f;
                    animationObj.Sample();
                    st.enabled = false;
                }
            }
        }
    }

    IEnumerator MoveToPosition(Vector3 target, float movingSpeed = 2f) {
        Vector3 startPos = animatedObject.position;
        float totalDist = Vector3.Distance(startPos, target);
        if (totalDist < 0.01f) yield break;

        float duration = totalDist / movingSpeed;
        float elapsed = 0f;

        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // smoothstep: 3t² − 2t³
            float smoothT = t * t * (3f - 2f * t);
            animatedObject.position = Vector3.Lerp(startPos, target, smoothT);
            yield return null;
        }

        animatedObject.position = target;
    }

    IEnumerator RotateToRotation(Quaternion targetRotation, float turnSpeed) {
        Quaternion startRot = animatedObject.rotation;
        float totalAngle = Quaternion.Angle(startRot, targetRotation);
        if (totalAngle < 0.5f) yield break;

        float duration = totalAngle / turnSpeed;
        float elapsed = 0f;

        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float smoothT = t * t * (3f - 2f * t);
            animatedObject.rotation = Quaternion.Slerp(startRot, targetRotation, smoothT);
            yield return null;
        }

        animatedObject.rotation = targetRotation;
    }



}
