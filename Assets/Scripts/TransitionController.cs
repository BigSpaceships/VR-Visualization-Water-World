using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionController : MonoBehaviour {
    private bool _justLoaded;
    
    private Coroutine _loadingCoroutine;   

    private TransitionTunnel _currentTunnel;

    public float loadStopTime;

    IEnumerator Start() {
        DontDestroyOnLoad(gameObject);
        
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
            _currentTunnel = tunnel;
            
            _loadingCoroutine = StartCoroutine(LoadScene(tunnel));
        }
        
        if (other == tunnel.transitionCollider) {
            StartCoroutine(SwitchScenes());
        }
        
    }

    private void OnTriggerExit(Collider other) {
        if (other == _currentTunnel.loadSceneCollider) {
            SceneManager.UnloadSceneAsync(_currentTunnel.transitionScene.name);
            
            StopCoroutine(_loadingCoroutine);
        }
    }

    private IEnumerator LoadScene(TransitionTunnel tunnel) {
        string sceneName = tunnel.transitionScene.name;
        
        var sceneLoadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        sceneLoadOperation.allowSceneActivation = false;

        yield return new WaitUntil(() => sceneLoadOperation.progress >= 0.9f);

        sceneLoadOperation.allowSceneActivation = true;
        
        yield return sceneLoadOperation;

        var oldTunnelTransform = tunnel.transform;

        var otherScene = SceneManager.GetSceneByName(tunnel.transitionScene.name);

        var otherTunnel = FindOtherTunnel(tunnel);
        var newTunnelTransform = otherTunnel.transform;
        
        otherTunnel.transitionCollider.gameObject.SetActive(false);
        otherTunnel.loadSceneCollider.gameObject.SetActive(false);
        
        var transformationMatrix = oldTunnelTransform.localToWorldMatrix * newTunnelTransform.worldToLocalMatrix;

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
    }

    private IEnumerator SwitchScenes() {
        _justLoaded = true;
        
        _currentTunnel.transitionCollider.gameObject.SetActive(false);
        _currentTunnel.loadSceneCollider.gameObject.SetActive(false);
        
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(_currentTunnel.transitionScene.name));
        
        _currentTunnel = FindOtherTunnel(_currentTunnel);
        
        _currentTunnel.transitionCollider.gameObject.SetActive(true);
        _currentTunnel.loadSceneCollider.gameObject.SetActive(true);

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