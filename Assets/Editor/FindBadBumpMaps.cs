#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class FindBadBumpMaps {
    [MenuItem("Tools/Find Cubemaps in BumpMap Slots")]
    public static void ScanMaterials() {
        int found = 0;
        // 全项目扫描所有材质
        var guids = AssetDatabase.FindAssets("t:Material");
        foreach (var g in guids) {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null && mat.HasProperty("_BumpMap")) {
                var tex = mat.GetTexture("_BumpMap");
                if (tex is Cubemap) {
                    Debug.LogError($"材质 `{mat.name}` 的 _BumpMap 插槽里放了 Cubemap：{tex.name}", mat);
                    found++;
                }
            }
        }

        if (found == 0)
            Debug.Log("✔️ 没有在 _BumpMap 找到任何 Cubemap。");
        else
            Debug.Log($"⚠️ 共找到 {found} 个错误材质，请检查 Console 列表定位并修正。");
    }
}
#endif