using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour
{
    CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = transform.GetChild(0).GetComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(FadeIn(0.5f));
    }

    public void myLoadScene(string sceneName)
    {
        StartCoroutine(Load(sceneName, 0.5f));
    }

    IEnumerator Load(string sceneName, float duration)
    {
        float elapsedTime = 0;
        float alpha = 0;
        while (alpha <= 1)
        {
            alpha = canvasGroup.alpha = elapsedTime / duration;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Scene currentScene = SceneManager.GetActiveScene();
        AsyncOperation load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!load.isDone) yield return null;
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
        SceneManager.UnloadSceneAsync(currentScene);
    }

    public void ReloadScene(float duration)
    {
        StartCoroutine(Reload(duration));
    }

    IEnumerator Reload(float duration)
    {
        float elapsedTime = 0;
        float alpha = 0;
        while (alpha <= 1)
        {
            alpha = canvasGroup.alpha = elapsedTime / duration;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
    }

    public static bool EqualVectors(Vector3 vector, Vector3 target, float threshold = 0.1f)
    {
        if (Mathf.Abs(vector.x - target.x) < threshold && Mathf.Abs(vector.y - target.y) < threshold && Mathf.Abs(vector.z - target.z) < threshold) return true;
        return false;
    }

    private IEnumerator FadeIn(float duration)
    {
        float elapsedTime = 0;
        float alpha = 1;
        while (alpha >= 0.0f)
        {
            alpha = canvasGroup.alpha = 1 - elapsedTime / duration;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}
