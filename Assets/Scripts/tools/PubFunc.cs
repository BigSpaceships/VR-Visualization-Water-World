using System;
using System.Collections.Generic;

public static class PubFunc {
    /// <summary>
    /// ������ "aniName=sss,sww;speed=2;" �Ĳ������������ֵ�
    /// </summary>
    /// <param name="paramString">���� "key1=val1;key2=val2;��;" ���ַ���</param>
    /// ʹ�÷�����paramMap.TryGetValue("aniName", out var names)
    /// <returns>Dictionary[key]=value</returns>
    public static Dictionary<string, string> ParseParams(string paramString) {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrEmpty(paramString))
            return dict;

        // ȥ��ͷβ�հ��Լ�ĩβ�ֺ�
        var trimmed = paramString.Trim().TrimEnd(';');

        // �� ';' �з�ÿһ�� key=value
        var pairs = trimmed.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var pair in pairs) {
            // ���������֣���ֹ value ���ٰ��� '='
            var kv = pair.Split(new[] { '=' }, 2);
            if (kv.Length != 2)
                continue;

            var key = kv[0].Trim();
            var val = kv[1].Trim();
            if (string.IsNullOrEmpty(key))
                continue;

            dict[key] = val;
        }

        return dict;
    }
}
