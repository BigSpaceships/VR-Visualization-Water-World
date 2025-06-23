using System.Collections;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class Manager : MonoBehaviour
{
    CanvasGroup resortCanvasGroup;
    public static CanvasGroup skiCanvasGroup;
    [SerializeField] GameObject devSim;
    [SerializeField] GameObject resortOrigin;
    [SerializeField] GameObject lights;
    [SerializeField] InputActionManager inputActionManager;
    [SerializeField] GameObject[] tooltips;
    ActionBasedContinuousMoveProvider continuousMoveProvider;
    [SerializeField] Transform scooter;
    [SerializeField] InputActionProperty reloadProperty;
    [SerializeField] InputAction reload;
    AudioSource audioSource;
    [SerializeField] AudioClip transitionClip;
    Transform resortCam;
    CharacterController characterController;
    Vector3 initialPos;
    Quaternion initialRot;
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        characterController = resortOrigin.GetComponent<CharacterController>();
        continuousMoveProvider = resortOrigin.GetComponent<ActionBasedContinuousMoveProvider>();
        resortCam = resortOrigin.transform.GetChild(0).GetChild(0);
        skiCanvasGroup = transform.GetChild(0).GetComponent<CanvasGroup>();
        resortCanvasGroup = TouchScreens.canvasGroup = transform.GetChild(1).GetComponent<CanvasGroup>();
        TouchScreens.manager = this;
        skiCanvasGroup.alpha = resortCanvasGroup.alpha = 0;
        reload = reloadProperty.action;
        resortOrigin.transform.GetPositionAndRotation(out initialPos, out initialRot);
        GameObject.FindWithTag("DivingTransition").GetComponent<Button>().onClick.AddListener(delegate { DivingMode(true); });
        GameObject.FindWithTag("SkiTransition").GetComponent<Button>().onClick.AddListener(delegate { SkiMode(true); });
/*#if UNITY_EDITOR
        devSim = Instantiate(devSim);
        DontDestroyOnLoad(devSim);
#endif*/
    }

    public static bool EqualVectors(Vector3 vector, Vector3 target, float threshold = 0.1f)
    {
        if (Mathf.Abs(vector.x - target.x) < threshold && Mathf.Abs(vector.y - target.y) < threshold && Mathf.Abs(vector.z - target.z) < threshold) return true;
        return false;
    }

    public void SkiMode(bool skiing)
    {
        audioSource.PlayOneShot(transitionClip);
        StartCoroutine(SkiingTransition(skiing, 0.5f));
    }

    void Update()
    {
        if (reload.WasPressedThisFrame() && SceneManager.GetSceneByName("R_Area2 Under Water").isLoaded) DivingMode(false);
    }

    IEnumerator SkiingTransition(bool skiing, float duration)
    {
        CanvasGroup fromCanvas;
        CanvasGroup toCanvas;
        if (skiing)
        {
            fromCanvas = resortCanvasGroup;
            toCanvas = skiCanvasGroup;
            resortOrigin.transform.GetPositionAndRotation(out initialPos, out initialRot);
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
            AsyncOperation op = SceneManager.LoadSceneAsync("Skiing");
            while (!op.isDone) yield return null;
            /*Scene activeScene = SceneManager.GetSceneByName("Skiing");
            SceneManager.SetActiveScene(activeScene);
            for (int i = 0; i < SceneManager.loadedSceneCount; i++) if (SceneManager.GetSceneAt(i) != activeScene) SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(i));*/
            yield return null;
            resortOrigin.SetActive(false);
            lights.SetActive(false);
            GameObject.FindWithTag("SceneTransition").GetComponent<Button>().onClick.AddListener(delegate { SkiMode(false); });
        }
        else
        {
            AsyncOperation op = SceneManager.LoadSceneAsync("R_Main");
            while (!op.isDone) yield return null;
            if (!SceneManager.GetSceneByName("R_Area1_LOW").isLoaded)
            {
                op = SceneManager.LoadSceneAsync("R_Area1_LOW", LoadSceneMode.Additive);
                while (!op.isDone) yield return null;
            }
            if (!SceneManager.GetSceneByName("R_Area2").isLoaded)
            {
                op = SceneManager.LoadSceneAsync("R_Area2", LoadSceneMode.Additive);
                while (!op.isDone) yield return null;
            }
            if (!SceneManager.GetSceneByName("R_Area3").isLoaded)
            {
                op = SceneManager.LoadSceneAsync("R_Area3", LoadSceneMode.Additive);
                while (!op.isDone) yield return null;
            }
            if (!SceneManager.GetSceneByName("R_Area4_LOW").isLoaded)
            {
                op = SceneManager.LoadSceneAsync("R_Area4_LOW", LoadSceneMode.Additive);
                while (!op.isDone) yield return null;
            }
            if (!SceneManager.GetSceneByName("R_Area5_LOW").isLoaded)
            {
                op = SceneManager.LoadSceneAsync("R_Area5_LOW", LoadSceneMode.Additive);
                while (!op.isDone) yield return null;
            }
            //SceneManager.SetActiveScene(SceneManager.GetSceneByName("R_Main"));
            //SceneManager.UnloadSceneAsync("Skiing");
            yield return null;
            resortOrigin.SetActive(true);
            lights.SetActive(true);
            resortOrigin.transform.SetPositionAndRotation(initialPos, initialRot);
            inputActionManager.actionAssets[0].Disable();
            inputActionManager.actionAssets[0].Enable();
/*#if UNITY_EDITOR
            devSim.SetActive(false);
            devSim.SetActive(true);
#endif*/
            GameObject.FindWithTag("DivingTransition").GetComponent<Button>().onClick.AddListener(delegate { DivingMode(true); });
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

    public void DivingMode(bool diving)
    {
        audioSource.PlayOneShot(transitionClip);
        StartCoroutine(DivingTransition(diving));
    }

    IEnumerator DivingTransition(bool diving, float duration = 0.5f)
    {
        if (diving) resortOrigin.transform.GetPositionAndRotation(out initialPos, out initialRot);
        float elapsedTime = 0;
        float alpha = 0;
        while (alpha <= 0.99f)
        {
            alpha = resortCanvasGroup.alpha = elapsedTime / duration;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        alpha = resortCanvasGroup.alpha = 1;
        elapsedTime = 0;
        if (diving)
        {
            AsyncOperation op = SceneManager.LoadSceneAsync("R_Area2 Under Water");
            while (!op.isDone) yield return null;
            /*Scene activeScene = SceneManager.GetSceneByName("R_Area2 Under Water");
            SceneManager.SetActiveScene(activeScene);
            for (int i = 0; i < SceneManager.loadedSceneCount; i++) if (SceneManager.GetSceneAt(i) != activeScene) SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(i));*/
            yield return null;
            characterController.enabled = continuousMoveProvider.useGravity = false;
            continuousMoveProvider.enableFly = true;
            continuousMoveProvider.forwardSource = scooter;
            for (int i = 0; i < tooltips.Length; i++) tooltips[i].SetActive(false);
        }
        else
        {
            AsyncOperation op = SceneManager.LoadSceneAsync("R_Main");
            while (!op.isDone) yield return null;
            if (!SceneManager.GetSceneByName("R_Area1_LOW").isLoaded)
            {
                op = SceneManager.LoadSceneAsync("R_Area1_LOW", LoadSceneMode.Additive);
                while (!op.isDone) yield return null;
            }
            if (!SceneManager.GetSceneByName("R_Area2").isLoaded)
            {
                op = SceneManager.LoadSceneAsync("R_Area2", LoadSceneMode.Additive);
                while (!op.isDone) yield return null;
            }
            if (!SceneManager.GetSceneByName("R_Area3").isLoaded)
            {
                op = SceneManager.LoadSceneAsync("R_Area3", LoadSceneMode.Additive);
                while (!op.isDone) yield return null;
            }
            if (!SceneManager.GetSceneByName("R_Area4_LOW").isLoaded)
            {
                op = SceneManager.LoadSceneAsync("R_Area4_LOW", LoadSceneMode.Additive);
                while (!op.isDone) yield return null;
            }
            if (!SceneManager.GetSceneByName("R_Area5_LOW").isLoaded)
            {
                op = SceneManager.LoadSceneAsync("R_Area5_LOW", LoadSceneMode.Additive);
                while (!op.isDone) yield return null;
            }
            yield return null;
            //SceneManager.SetActiveScene(SceneManager.GetSceneByName("R_Main"));
            //SceneManager.UnloadSceneAsync("R_Area2 Under Water");
            resortOrigin.transform.SetPositionAndRotation(initialPos, initialRot);
            characterController.enabled = continuousMoveProvider.useGravity = true;
            continuousMoveProvider.enableFly = false;
            continuousMoveProvider.forwardSource = resortCam;
            inputActionManager.actionAssets[0].Disable();
            inputActionManager.actionAssets[0].Enable();
/*#if UNITY_EDITOR
            devSim.SetActive(false);
            devSim.SetActive(true);
#endif*/
            GameObject.FindWithTag("DivingTransition").GetComponent<Button>().onClick.AddListener(delegate { DivingMode(true); });
            GameObject.FindWithTag("SkiTransition").GetComponent<Button>().onClick.AddListener(delegate { SkiMode(true); });
            for (int i = 0; i < tooltips.Length; i++) tooltips[i].SetActive(true);
        }
        while (alpha >= 0.01f)
        {
            alpha = resortCanvasGroup.alpha = 1 - elapsedTime / duration;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        resortCanvasGroup.alpha = 0;
    }
}
