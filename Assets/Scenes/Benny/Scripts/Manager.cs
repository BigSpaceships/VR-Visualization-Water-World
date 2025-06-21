using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Manager : MonoBehaviour
{
    public CanvasGroup resortCanvasGroup;
    public CanvasGroup skiCanvasGroup;
    [SerializeField] GameObject resortScene;
    [SerializeField] GameObject skiScene;
    [SerializeField] GameObject devSim;

    [Header("Resort Components")]
    [SerializeField] CharacterController characterController;
    [SerializeField] PC_MouseLook pC_MouseLook;
    [SerializeField] ActionBasedContinuousMoveProvider continuousMoveProvider;
    [SerializeField] GameObject xrParent;
    [SerializeField] Collider parentCol;
    [SerializeField] Rigidbody parentRb;
    [SerializeField] GameObject camOffset;
    [SerializeField] GameObject locomotionSystem;
    Vector3 initialPos;
    Quaternion initialRot;

    [Header("Skiing Components")]
    [SerializeField] CapsuleCollider col;
    [SerializeField] Skier skier;
    AudioSource[] audioSources;
    [SerializeField] GameObject xrOrigin;
    [SerializeField] GameObject interactableParent;
    [SerializeField] GameObject skiCamOffset;
    Rigidbody xrRb;
    void Awake()
    {
        skiScene.SetActive(false);
        skiCanvasGroup = transform.GetChild(0).GetComponent<CanvasGroup>();
        resortCanvasGroup = transform.GetChild(1).GetComponent<CanvasGroup>();
        skiCanvasGroup.alpha = resortCanvasGroup.alpha = 0;
        audioSources = xrOrigin.GetComponents<AudioSource>();
        xrOrigin.transform.GetPositionAndRotation(out initialPos, out initialRot);
#if UNITY_EDITOR
        Instantiate(devSim);
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
        if (skiing)
        {
            resortScene.SetActive(false);
            SwitchXROrigin(true);
            skiScene.SetActive(true);
        }
        else
        {
            skiScene.SetActive(false);
            SwitchXROrigin(false);
            resortScene.SetActive(true);
        }
        elapsedTime = 0;
        yield return new WaitForSeconds(1);
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
