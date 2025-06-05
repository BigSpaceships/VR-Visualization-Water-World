using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TouchScreens : MonoBehaviour {
    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public void OnButtonClick(string action) {
        Debug.Log(action);
        switch (action) {
            case "StartDive":
                StartCoroutine(SwitchScene());
                break;

            default:
                Debug.LogWarning("Unknown action: " + action);
                break;
        }
    }

    private IEnumerator SwitchScene() {
        // �����³���
        AsyncOperation loadOp = SceneManager.LoadSceneAsync("R_Area2 Under Water", LoadSceneMode.Additive);
        while (!loadOp.isDone)
            yield return null;

        yield return null; // �ȴ�һ֡���ȶ�״̬

        // ж�ؾɳ���Area0��������ɾ��
        //AsyncOperation unloadOp = SceneManager.UnloadSceneAsync("TestSwitchScene");
        //while (!unloadOp.isDone)
        //    yield return null;
    }

}
