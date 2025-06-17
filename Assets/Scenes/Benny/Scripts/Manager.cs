using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour
{
    public void myLoadScene(string sceneName)
    {
        StartCoroutine(Load(sceneName));
    }

    IEnumerator Load(string sceneName)
    {
        Scene currentScene = SceneManager.GetActiveScene();
        AsyncOperation load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!load.isDone) yield return null;
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
        SceneManager.UnloadSceneAsync(currentScene);
    }

    public static bool EqualVectors(Vector3 vector, Vector3 target, float threshold = 0.1f)
    {
        if (Mathf.Abs(vector.x - target.x) < threshold && Mathf.Abs(vector.y - target.y) < threshold && Mathf.Abs(vector.z - target.z) < threshold) return true;
        return false;
    }
}
