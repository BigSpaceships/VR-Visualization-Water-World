using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionController : MonoBehaviour {
    private bool _shouldTransition;

    private bool _justLoaded;

    public float loadStopTime;

    IEnumerator Start() {
        DontDestroyOnLoad(gameObject);

        _shouldTransition = false;
        
        _justLoaded = true;
        
        yield return new WaitForSeconds(loadStopTime);
        
        _justLoaded = false;
    }

    private void OnTriggerEnter(Collider other) {
        if (_justLoaded) {
            return;
        }
        
        var tunnel = other.GetComponentInParent<TransitionTunnel>();

        if (other == tunnel.loadSceneCollider) {
            StartCoroutine(LoadScene(tunnel.transitionScene.name));
        }

        if (other == tunnel.transitionCollider) {
            _shouldTransition = true;
        }
    }

    private IEnumerator LoadScene(string sceneName) {
        var currentScene = SceneManager.GetActiveScene();
        
        var sceneLoadOperation = SceneManager.LoadSceneAsync(sceneName);

        sceneLoadOperation.allowSceneActivation = false;
        
        Debug.Log("loading scene");

        yield return new WaitUntil(() => sceneLoadOperation.progress >= 0.9f);
        
        Debug.Log("scene loaded");

        yield return new WaitUntil(() => _shouldTransition);
        
        Debug.Log("transitioning");
        
        _shouldTransition = false;
        _justLoaded = true;

        sceneLoadOperation.allowSceneActivation = true;
        
        yield return sceneLoadOperation;
        Debug.Log("unloading other scene");

        for (int i = 0; i < SceneManager.loadedSceneCount; i++) {
            if (SceneManager.GetSceneAt(i) == currentScene) {
                SceneManager.UnloadSceneAsync(currentScene);
            }
        }

        var openControllers = GameObject.FindGameObjectsWithTag("Player");

        foreach (var openController in openControllers) {
            if (openController != gameObject) {
                Destroy(openController);
            }
        }

        yield return new WaitForSeconds(loadStopTime);
        
        _justLoaded = false;
    }
}