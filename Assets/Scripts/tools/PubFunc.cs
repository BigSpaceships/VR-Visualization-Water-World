using System;
using System.Collections.Generic;

public static class PubFunc {
    /// <summary>
    /// 将类似 "aniName=sss,sww;speed=2;" 的参数串解析成字典
    /// </summary>
    /// <param name="paramString">形如 "key1=val1;key2=val2;…;" 的字符串</param>
    /// 使用方法：paramMap.TryGetValue("aniName", out var names)
    /// <returns>Dictionary[key]=value</returns>
    public static Dictionary<string, string> ParseParams(string paramString) {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrEmpty(paramString))
            return dict;

        // 去掉头尾空白以及末尾分号
        var trimmed = paramString.Trim().TrimEnd(';');

        // 按 ';' 切分每一对 key=value
        var pairs = trimmed.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var pair in pairs) {
            // 最多分两部分，防止 value 里再包含 '='
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
