using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TransitionTunnel : MonoBehaviour {
#if UNITY_EDITOR
    [Tooltip("在编辑器中选择场景资源")]
    public SceneAsset transitionSceneEditorOnly; // ✅ 仅编辑器使用，用来选择场景
#endif

    [Tooltip("实际用于加载的场景名，会在构建时使用")]
    public string transitionScene; // ✅ 构建时使用

    public string tunnelName;
    public Collider loadSceneCollider;
    public Collider transitionCollider;

#if UNITY_EDITOR
    // 在编辑器中自动同步 scene 名称到字符串字段
    private void OnValidate() {
        if (transitionSceneEditorOnly != null) {
            transitionScene = transitionSceneEditorOnly.name;
            UnityEditor.EditorUtility.SetDirty(this); // 确保修改被保存
        }
    }
#endif
}