using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class Manager : MonoBehaviour {
    public static Manager instance;

    [HideInInspector] public CanvasGroup resortCanvasGroup;
    public static CanvasGroup skiCanvasGroup;
    //[SerializeField] GameObject devSim;
    [SerializeField] GameObject resortOrigin;
    [SerializeField] InputActionManager inputActionManager;
    [SerializeField] GameObject[] tooltips;
    [SerializeField] InputActionProperty reloadProperty;
    [SerializeField] InputAction reload;
    AudioSource audioSource;
    [SerializeField] AudioClip transitionClip;
    Transform resortCam;
    CharacterController characterController;
    void Awake() {
        instance = this;
        audioSource = GetComponent<AudioSource>();
        characterController = resortOrigin.GetComponent<CharacterController>();
        resortCam = resortOrigin.transform.GetChild(0).GetChild(0); //main camera
        skiCanvasGroup = transform.GetChild(0).GetComponent<CanvasGroup>();
        resortCanvasGroup = transform.GetChild(1).GetComponent<CanvasGroup>();
        skiCanvasGroup.alpha = resortCanvasGroup.alpha = 0;
        reload = reloadProperty.action;
        GameObject.FindWithTag("SkiTransition").GetComponent<Button>().onClick.AddListener(delegate { SkiMode(true); });
    }

    public static bool EqualVectors(Vector3 vector, Vector3 target, float threshold = 0.1f) {
        if (Mathf.Abs(vector.x - target.x) < threshold && Mathf.Abs(vector.y - target.y) < threshold && Mathf.Abs(vector.z - target.z) < threshold) return true;
        return false;
    }

    public void Teleport() {

    }

    public void SkiMode(bool skiing) {
        audioSource.PlayOneShot(transitionClip);
        StartCoroutine(SkiingTransition(skiing, 0.5f));
    }

    IEnumerator SkiingTransition(bool skiing, float duration) {
        CanvasGroup fromCanvas;
        CanvasGroup toCanvas;
        if (skiing) {
            characterController.enabled = true;
            fromCanvas = resortCanvasGroup;
            toCanvas = skiCanvasGroup;
        } else {
            fromCanvas = skiCanvasGroup;
            toCanvas = resortCanvasGroup;
        }
        float elapsedTime = 0;
        float alpha = 0;
        while (alpha <= 0.99f) {
            alpha = fromCanvas.alpha = elapsedTime / duration;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        alpha = fromCanvas.alpha = toCanvas.alpha = 1;
        elapsedTime = 0;
        if (skiing) {
            GamePublicV2.instance.savePlayerPosition();
            AreaLoaderController loader = GameObject.Find("AreaBorders").GetComponent<AreaLoaderController>();
            yield return StartCoroutine(loader.UnloadAllScenesCoroutine());
            AsyncOperation op = SceneManager.LoadSceneAsync("R_Area3 Skiing", LoadSceneMode.Additive);
            while (!op.isDone) yield return null;
            Skier skier = resortOrigin.GetComponent<Skier>();
            skier.activeSki();
            yield return null;
            skiCanvasGroup.GetComponent<Canvas>().worldCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
            Skier.canvasGroup = skiCanvasGroup;
            GameObject.FindWithTag("SceneTransition").GetComponent<Button>().onClick.AddListener(delegate { SkiMode(false); });
        } else {
            AreaLoaderController loader = GameObject.Find("AreaBorders").GetComponent<AreaLoaderController>();
            yield return StartCoroutine(loader.UnloadScene("R_Area3 Skiing"));
            yield return StartCoroutine(loader.RefreshCoroutine());
            GamePublicV2.instance.setMoveMode(MoveMode.Ground);
            GamePublicV2.instance.setController(ControllerName.Main);
            GamePublicV2.instance.loadPlayerPosition();
            //inputActionManager.actionAssets[0].Disable();
            //inputActionManager.actionAssets[0].Enable();
            /*#if UNITY_EDITOR
                        //devSim.SetActive(false);
                        //devSim.SetActive(true);
            #endif*/
            GameObject.FindWithTag("SkiTransition").GetComponent<Button>().onClick.AddListener(delegate { SkiMode(true); });
        }
        while (alpha >= 0.01f) {
            alpha = toCanvas.alpha = 1 - elapsedTime / duration;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        toCanvas.alpha = fromCanvas.alpha = 0;
        if (skiing) Skier.initialized = true;
    }
}
