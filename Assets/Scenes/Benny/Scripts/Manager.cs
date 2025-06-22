using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class Manager : MonoBehaviour
{
    CanvasGroup resortCanvasGroup;
    public static CanvasGroup skiCanvasGroup;
    [SerializeField] GameObject devSim;
    [SerializeField] GameObject resortOrigin;
    [SerializeField] GameObject lights;
    [SerializeField] InputActionManager inputActionManager;
    Vector3 initialPos;
    Quaternion initialRot;
    void Awake()
    {
        skiCanvasGroup = transform.GetChild(0).GetComponent<CanvasGroup>();
        resortCanvasGroup = TouchScreens.canvasGroup = transform.GetChild(1).GetComponent<CanvasGroup>();
        skiCanvasGroup.alpha = resortCanvasGroup.alpha = 0;
        resortOrigin.transform.GetPositionAndRotation(out initialPos, out initialRot);
#if UNITY_EDITOR
        devSim = Instantiate(devSim);
        DontDestroyOnLoad(devSim);
#endif
    }

    public static bool EqualVectors(Vector3 vector, Vector3 target, float threshold = 0.1f)
    {
        if (Mathf.Abs(vector.x - target.x) < threshold && Mathf.Abs(vector.y - target.y) < threshold && Mathf.Abs(vector.z - target.z) < threshold) return true;
        return false;
    }

    public void SkiMode(bool skiing)
    {
        StartCoroutine(SkiingTransition(skiing, 0.5f));
    }

    IEnumerator SkiingTransition(bool skiing, float duration)
    {
        CanvasGroup fromCanvas;
        CanvasGroup toCanvas;
        if (skiing)
        {
            fromCanvas = resortCanvasGroup;
            toCanvas = skiCanvasGroup;
        }
        else
        {
            fromCanvas = skiCanvasGroup;
            toCanvas = resortCanvasGroup;
        }
        float elapsedTime = 0;
        float alpha = 0;
        while (alpha <= 0.99f)
        {
            alpha = fromCanvas.alpha = elapsedTime / duration;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        alpha = fromCanvas.alpha = toCanvas.alpha = 1;
        elapsedTime = 0;
        if (skiing)
        {
            AsyncOperation op = SceneManager.LoadSceneAsync("Skiing", LoadSceneMode.Additive);
            while (!op.isDone) yield return null;
            yield return null;
            Scene activeScene = SceneManager.GetSceneByName("Skiing");
            SceneManager.SetActiveScene(activeScene);
            int sceneCount = SceneManager.sceneCount;
            for (int i = 0; i < sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene != activeScene) SceneManager.UnloadSceneAsync(scene);
            }
            resortOrigin.SetActive(false);
            lights.SetActive(false);
            GameObject.FindWithTag("SceneTransition").GetComponent<Button>().onClick.AddListener(delegate { SkiMode(false); } );
        }
        else
        {
            AsyncOperation op = SceneManager.LoadSceneAsync("R_Main", LoadSceneMode.Additive);
            while (!op.isDone) yield return null;
            yield return null;
            SceneManager.SetActiveScene(SceneManager.GetSceneByName("R_Main"));
            SceneManager.UnloadSceneAsync("Skiing");
            resortOrigin.SetActive(true);
            lights.SetActive(true);
            resortOrigin.transform.SetPositionAndRotation(initialPos, initialRot);
            inputActionManager.actionAssets[0].Disable();
            inputActionManager.actionAssets[0].Enable();
#if UNITY_EDITOR
            devSim.SetActive(false);
            devSim.SetActive(true);
#endif
            GameObject.FindWithTag("SkiTransition").GetComponent<Button>().onClick.AddListener(delegate { SkiMode(true); });
        }
        while (alpha >= 0.01f)
        {
            alpha = toCanvas.alpha = 1 - elapsedTime / duration;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        toCanvas.alpha = fromCanvas.alpha = 0;
        if (skiing) Skier.initialized = true;
    }
}
