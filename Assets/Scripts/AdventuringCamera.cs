using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class AdventuringCamera : MonoBehaviour {
    [Header("Camera Settings")]
    [SerializeField] private Camera displayCamera;
    [SerializeField] private RawImage screenImage;

    [SerializeField] private Vector2Int displayResolution;

    [Header("Controls")]
    public InputActionProperty zoomAction = new InputActionProperty(new InputAction("Zoom"));
    public InputActionProperty takePictureAction = new InputActionProperty(new InputAction("Take Picture"));
    private float zoom;
    [SerializeField] private Vector2 zoomRange;
    [SerializeField] private float zoomSpeed;
    
    [Header("Effect Settings")]
    [SerializeField] private float darkTime;

    private bool active;

    private RenderTexture displayRenderTexture;

    public List<RenderTexture> takenPictures;

    private void Start() {
        takenPictures = new List<RenderTexture>();

        displayRenderTexture = new RenderTexture(displayResolution.x, displayResolution.y, 8, RenderTextureFormat.ARGB32);

        screenImage.texture = displayRenderTexture;

        displayCamera.targetTexture = displayRenderTexture;

        zoom = displayCamera.fieldOfView;
    }

    public void OnPickup() {
        active = true;

        takePictureAction.action.performed += TakePictureAction;
    }

    public void OnDrop() {
        active = false;

        takePictureAction.action.performed -= TakePictureAction;
    }

    private void Update() {
        if (active) {
            var zoomValue = zoomAction.action.ReadValue<Vector2>();
            var zoomChange = zoomValue.y;

            zoom += -zoomChange * zoomSpeed;

            zoom = Mathf.Clamp(zoom, zoomRange.x, zoomRange.y);

            displayCamera.fieldOfView = zoom;
        }
    }

    private void TakePictureAction(InputAction.CallbackContext obj) {
        StartCoroutine(TakePicture());
    }

    public IEnumerator TakePicture() {
        var newTexture = new RenderTexture(displayRenderTexture);
        displayCamera.targetTexture = newTexture;
        displayCamera.Render();

        takenPictures.Add(newTexture);

        displayCamera.targetTexture = displayRenderTexture;
        return null;
    }
}