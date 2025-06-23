using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TouchScreens : MonoBehaviour {
    //This canvas group is assigned in the manager script on the persistent ski manager object

    // Start is called before the first frame update
    /*void Start() {

    }

    // Update is called once per frame
    void Update() {
        
        
        
        //鼠标点击测试代码
        /*
        if (Mouse.current == null) return;
        if (Mouse.current.leftButton.wasPressedThisFrame) {
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = Mouse.current.position.ReadValue();

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            if (results.Count == 0) {
                Debug.LogWarning("❌ Mouse click did NOT hit any UI element.");
            } else {
                Debug.Log($"✅ UI hit count: {results.Count}");
                foreach (RaycastResult result in results) {
                    Debug.Log($"🎯 Hit UI element: {result.gameObject.name}");
                }
            }
        }
    }*/

    public void OnButtonClick(string action) {
        switch (action) {
            case "StartDive":
                StartCoroutine(SwitchScene());
                break;
            case "TTest":
                GameObject player = GameObject.Find("PersistentXR/XR Origin");
                GameObject dest = GameObject.Find("PersistentXR/currentTestPoint");
                player.transform.position = dest.transform.position;
                break;
            case "changeController":
                if (GamePublicV2.instance.currentControllerName == ControllerName.Main) {
                    GamePublicV2.instance.setController(ControllerName.A2);
                } else {
                    GamePublicV2.instance.setController(ControllerName.Main);
                }
                break;
            default:
                //Debug.LogWarning("Unknown action: " + action);
                break;
        }
    }

    private IEnumerator SwitchScene(float duration = 0.5f)
    {
        //load new scene
        AsyncOperation loadOp = SceneManager.LoadSceneAsync("R_Area2 Under Water", LoadSceneMode.Additive);
        while (!loadOp.isDone) yield return null;
        yield return null;
    }

}
