using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TransitionTunnel : MonoBehaviour {
    public SceneAsset transitionScene;

    public string tunnelName;
    
    public Collider loadSceneCollider;
    public Collider transitionCollider;
}