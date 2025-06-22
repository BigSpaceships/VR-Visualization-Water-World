using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using Unity.XR.CoreUtils; // XR Origin ���������ռ�
using System.Collections.Generic;
using System.Collections;
using UnityEngine.XR.Management;

public class MenuManager : MonoBehaviour {
    void Awake() {
        DontDestroyOnLoad(this.gameObject);
    }

    public void LoadScene(string sceneName) {
        StartCoroutine(SwitchSceneAndRestartXR(sceneName));
    }

    IEnumerator SwitchSceneAndRestartXR(string sceneName) {
        // Stop XR before loading new scene
        //Debug.Log("[XR] Stopping XR...");
        XRGeneralSettings.Instance.Manager.StopSubsystems();
        XRGeneralSettings.Instance.Manager.DeinitializeLoader();

        yield return null;

        //Debug.Log("[Scene] Loading scene: " + sceneName);
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);

        // Wait one frame for scene to load
        yield return null;

        //("[XR] Initializing XR...");
        yield return XRGeneralSettings.Instance.Manager.InitializeLoader();

        if (XRGeneralSettings.Instance.Manager.activeLoader == null) {
            //Debug.Log("[XR] XR Loader failed to initialize.");
            yield break;
        }

        XRGeneralSettings.Instance.Manager.StartSubsystems();
        //Debug.Log("[XR] XR restarted.");

        // Ensure new XR Origin tracks properly
        var xrOrigin = FindObjectOfType<XROrigin>();
        if (xrOrigin != null) {
            //Debug.Log("[XR] Found new XR Origin.");
            SetTrackingOrigin();
        } else {
            //Debug.Log("[XR] No XR Origin found in new scene!");
        }
    }

    void SetTrackingOrigin() {
        var list = new List<XRInputSubsystem>();
        SubsystemManager.GetSubsystems(list);
        foreach (var system in list) {
            system.TrySetTrackingOriginMode(TrackingOriginModeFlags.Floor);
        }
    }
}
