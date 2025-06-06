using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class KeyValuePairData {
    public string key;
    public string value;
}

public class ObjectData : MonoBehaviour {
    public List<KeyValuePairData> data = new List<KeyValuePairData>();

    // 可选：获取方法
    public string Get(string key) {
        foreach (var kv in data)
            if (kv.key == key) return kv.value;
        return null;
    }
    public float GetFloat(string key, float defaultValue = 0f) {
        foreach (var pair in data) {
            if (pair.key == key && float.TryParse(pair.value, out float result))
                return result;
        }
        return defaultValue;
    }

    public void Set(string key, string value) {
        foreach (var kv in data) {
            if (kv.key == key) {
                kv.value = value;
                return;
            }
        }
        data.Add(new KeyValuePairData { key = key, value = value });
    }
}