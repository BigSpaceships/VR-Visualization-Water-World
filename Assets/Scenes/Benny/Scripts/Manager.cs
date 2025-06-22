using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class Manager : MonoBehaviour
{
    CanvasGroup resortCanvasGroup;
    public static CanvasGroup skiCanvasGroup;
    [SerializeField] GameObject devSim;

    [SerializeField] GameObject xrParent;
    [SerializeField] GameObject resortLights;
    CharacterController characterController;
    PC_MouseLook pC_MouseLook;
    ActionBasedContinuousMoveProvider continuousMoveProvider;
    Collider parentCol;
    Rigidbody parentRb;
    GameObject camOffset;
    GameObject locomotionSystem;
    Vector3 initialPos;
    Quaternion initialRot;

    [SerializeField] GameObject xrOrigin;
    CapsuleCollider col;
    Skier skier;
    AudioSource[] audioSources;
    GameObject interactableParent;
    GameObject skiCamOffset;
    Rigidbody xrRb;
    void Awake()
    {
        skiCanvasGroup = transform.GetChild(0).GetComponent<CanvasGroup>();
        resortCanvasGroup = transform.GetChild(1).GetComponent<CanvasGroup>();
        skiCanvasGroup.alpha = resortCanvasGroup.alpha = 0;
        characterController = xrOrigin.GetComponent<CharacterController>();
        pC_MouseLook = xrOrigin.GetComponent<PC_MouseLook>();
        continuousMoveProvider = xrOrigin.GetComponent<ActionBasedContinuousMoveProvider>();
        col = xrOrigin.GetComponent<CapsuleCollider>();
        skier = xrOrigin.GetComponent<Skier>();
        audioSources = xrOrigin.GetComponents<AudioSource>();
        camOffset = xrOrigin.transform.GetChild(0).gameObject;
        locomotionSystem = xrOrigin.transform.GetChild(1).gameObject;
        interactableParent = xrOrigin.transform.GetChild(2).gameObject;
        skiCamOffset = xrOrigin.transform.GetChild(3).gameObject;
        parentCol = xrParent.GetComponent<Collider>();
        parentRb = xrParent.GetComponent<Rigidbody>();
#if UNITY_EDITOR
        DontDestroyOnLoad(Instantiate(devSim));
#endif
    }

    void Start()
    {
        xrOrigin.transform.GetPositionAndRotation(out initialPos, out initialRot);
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
        SwitchXROrigin(skiing);
        elapsedTime = 0;
        if (skiing)
        {
            AsyncOperation op = SceneManager.LoadSceneAsync("Skiing", LoadSceneMode.Additive);
            while (!op.isDone) yield return null;
            SceneManager.SetActiveScene(SceneManager.GetSceneByName("Skiing"));
            SceneManager.UnloadSceneAsync("R_Main");
            SceneManager.UnloadSceneAsync("R_Area1_LOW");
            SceneManager.UnloadSceneAsync("R_Area2");
            SceneManager.UnloadSceneAsync("R_Area3");
            SceneManager.UnloadSceneAsync("R_Area4_LOW");
            SceneManager.UnloadSceneAsync("R_Area5_LOW");
            GameObject.FindWithTag("SceneTransition").GetComponent<Button>().onClick.AddListener(delegate { SkiMode(false); });
        }
        else
        {
            AsyncOperation op = SceneManager.LoadSceneAsync("R_Main", LoadSceneMode.Additive);
            while (!op.isDone) yield return null;
            SceneManager.SetActiveScene(SceneManager.GetSceneByName("R_Main"));
            SceneManager.UnloadSceneAsync("Skiing");
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

    void SwitchXROrigin(bool skiing)
    {
        col.enabled = skier.enabled = skiing;
        characterController.enabled = pC_MouseLook.enabled = continuousMoveProvider.enabled = parentCol.enabled = !skiing;
        skiCamOffset.SetActive(skiing);
        camOffset.SetActive(!skiing);
        interactableParent.SetActive(skiing);
        locomotionSystem.SetActive(!skiing);
        resortLights.SetActive(!skiing);
        for (int i = 0; i < audioSources.Length; i++) audioSources[i].enabled = skiing;
        if (skiing)
        {
            Destroy(parentRb);
            Skier.rb = Pole.skier = xrRb = xrOrigin.AddComponent<Rigidbody>();
            xrRb.mass = 80;
            xrRb.drag = xrRb.angularDrag = 3;
            xrRb.centerOfMass = new Vector3(0, 0.1f, 0);
            xrRb.interpolation = RigidbodyInterpolation.Interpolate;
            xrRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            xrRb.freezeRotation = true;
        }
        else
        {
            Destroy(xrRb);
            parentRb = xrParent.AddComponent<Rigidbody>();
            parentRb.freezeRotation = true;
            xrOrigin.transform.SetPositionAndRotation(initialPos, initialRot);
        }
    }
}
