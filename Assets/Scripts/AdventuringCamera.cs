using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

public class AdventuringCamera : MonoBehaviour {
    [SerializeField] private Camera displayCamera;
    [SerializeField] private RawImage screenImage;

    [SerializeField] private Vector2Int displayResolution;

    private RenderTexture _displayRenderTexture;

    public List<RenderTexture> takenPictures;

    private void Start() {
        takenPictures = new List<RenderTexture>();

        _displayRenderTexture = new RenderTexture(displayResolution.x, displayResolution.y, 0, RenderTextureFormat.ARGB32);

        screenImage.texture = _displayRenderTexture;

        displayCamera.targetTexture = _displayRenderTexture;
    }

    public void TakePicture() {
        Debug.Log("taking picture");
        var newTexture = new RenderTexture(_displayRenderTexture);
        displayCamera.targetTexture = newTexture;
        displayCamera.Render();

        takenPictures.Add(newTexture);

        displayCamera.targetTexture = _displayRenderTexture;
    }
}