using UnityEngine;
using UnityEditor;
using System.Collections;

public class LightmapBakingScript
{
    // 方法用于通过菜单调用
    [MenuItem("Tools/Bake Lightmaps")]
    public static void Bake()
    {
        // 清除现有的光照贴图
        Lightmapping.Clear();
        // 开始烘焙光照贴图
        Lightmapping.BakeAsync();
    }
}
