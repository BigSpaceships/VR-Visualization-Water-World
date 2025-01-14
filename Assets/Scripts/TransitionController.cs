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
            StartCoroutine(LoadScene(tunnel));
        }

        if (other == tunnel.transitionCollider) {
            _shouldTransition = true;
        }
    }

    private IEnumerator LoadScene(TransitionTunnel tunnel) {
        string sceneName = tunnel.transitionScene.name;
        
        var currentScene = SceneManager.GetActiveScene();
        
        var sceneLoadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        sceneLoadOperation.allowSceneActivation = false;

        yield return new WaitUntil(() => sceneLoadOperation.progress >= 0.9f);

        yield return new WaitUntil(() => _shouldTransition);
        
        _shouldTransition = false;
        _justLoaded = true;

        sceneLoadOperation.allowSceneActivation = true;
        
        yield return sceneLoadOperation;

        var oldTunnelTransform = tunnel.transform;
        
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));

        var otherScene = SceneManager.GetActiveScene();
        
        Debug.Log(otherScene.name);

        var otherTunnel = FindOtherTunnel(tunnel);
        var newTunnelTransform = otherTunnel.transform;
        
        var transformationMatrix = oldTunnelTransform.localToWorldMatrix * newTunnelTransform.worldToLocalMatrix;
        
        SceneManager.UnloadSceneAsync(currentScene.name);

        var sceneObjects = otherScene.GetRootGameObjects();

        foreach (var obj in sceneObjects) {
            obj.transform.position = transformationMatrix.MultiplyPoint(obj.transform.position);
            obj.transform.rotation = transformationMatrix.rotation * obj.transform.rotation;
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

    private TransitionTunnel FindOtherTunnel(TransitionTunnel tunnel) {
        var tunnelGameObjects = GameObject.FindGameObjectsWithTag("TransitionTunnel");
        
        var tunnels = new List<TransitionTunnel>();

        foreach (var tunnelGameObject in tunnelGameObjects) {
            var otherTunnel = tunnelGameObject.GetComponent<TransitionTunnel>();

            if (otherTunnel != tunnel && otherTunnel.name.Equals(tunnel.name)) {
                tunnels.Add(otherTunnel);
            }
        }

        return tunnels[0];
    }
}