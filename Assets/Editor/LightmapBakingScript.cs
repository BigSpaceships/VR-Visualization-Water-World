using UnityEngine;
using UnityEditor;
using System.Collections;

public class LightmapBakingScript
{
    // ��������ͨ���˵�����
    [MenuItem("Tools/Bake Lightmaps")]
    public static void Bake()
    {
        // ������еĹ�����ͼ
        Lightmapping.Clear();
        // ��ʼ�決������ͼ
        Lightmapping.BakeAsync();
    }
}
