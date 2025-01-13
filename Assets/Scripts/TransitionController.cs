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
        
        var sceneLoadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        sceneLoadOperation.allowSceneActivation = false;

        yield return new WaitUntil(() => sceneLoadOperation.progress >= 0.9f);

        yield return new WaitUntil(() => _shouldTransition);
        
        _shouldTransition = false;
        _justLoaded = true;

        sceneLoadOperation.allowSceneActivation = true;
        
        yield return sceneLoadOperation;
        
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));

        SceneManager.UnloadSceneAsync(currentScene.name);

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