using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Experimental.Rendering;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class AdventuringCamera : MonoBehaviour {
    [Header("Camera Settings")] [SerializeField]
    private Camera displayCamera;

    [SerializeField] private RawImage screenImage;

    [SerializeField] private Vector2Int displayResolution;

    [Header("Controls")] public InputActionProperty zoomAction = new InputActionProperty(new InputAction("Zoom"));
    public InputActionProperty takePictureAction = new InputActionProperty(new InputAction("Take Picture"));
    private float zoom;
    [SerializeField] private Vector2 zoomRange;
    [SerializeField] private float zoomSpeed;

    [Header("Effect Settings")] [SerializeField]
    private float darkTime;

    private bool active;

    private RenderTexture displayRenderTexture;

    public List<RenderTexture> takenPictures;

    public List<PictureDisplayTile.ObjectPictureInformation> scannedObjects;

    public UnityEvent<PictureDisplayTile.ObjectPictureInformation> onPictureTaken;

    private void Start() {
        takenPictures = new List<RenderTexture>();

        displayRenderTexture =
            new RenderTexture(displayResolution.x, displayResolution.y, 8, RenderTextureFormat.ARGB32);

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

    private void ScanForObjects(RenderTexture picture) {
        var scanableObjects = FindObjectsByType<ScanableObject>(FindObjectsSortMode.None);

        var frustumPlanes = GeometryUtility.CalculateFrustumPlanes(displayCamera);

        var objectsInPicture = new List<ScanableObject>();

        foreach (var scanableObject in scanableObjects) {
            var isVisible = GeometryUtility.TestPlanesAABB(frustumPlanes, scanableObject.collider.bounds);

            if (isVisible) {
                objectsInPicture.Add(scanableObject);

                if (!scannedObjects.Exists(information => information.scannedObject == scanableObject)) {
                    scanableObject.Photograph();
                }
            }
        }

        foreach (var obj in objectsInPicture) {
            var pictureInfo = new PictureDisplayTile.ObjectPictureInformation
                { pictureTaken = picture, scannedObject = obj };

            scannedObjects.Add(pictureInfo);

            onPictureTaken.Invoke(pictureInfo);
        }

        if (objectsInPicture.Count == 0) {
            var pictureInfo = new PictureDisplayTile.ObjectPictureInformation
                { pictureTaken = picture, scannedObject = null };

            scannedObjects.Add(pictureInfo);

            onPictureTaken.Invoke(pictureInfo);
        }
    }

    private void TakePictureAction(InputAction.CallbackContext obj) {
        StartCoroutine(TakePicture());
    }

    public IEnumerator TakePicture() {
        var newTexture = new RenderTexture(displayRenderTexture);
        displayCamera.targetTexture = newTexture;

        displayCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("ScanableObjectLabel"));

        displayCamera.Render();

        takenPictures.Add(newTexture);

        displayCamera.targetTexture = displayRenderTexture;

        displayCamera.cullingMask |= 1 << LayerMask.NameToLayer("ScanableObjectLabel");

        ScanForObjects(newTexture);

        var timePictureStarted = Time.time;

        while (Time.time < timePictureStarted + darkTime) {
            screenImage.color = Color.Lerp(Color.white, Color.black, (Time.time - timePictureStarted) / darkTime);

            yield return null;
        }

        timePictureStarted = Time.time;

        while (Time.time < timePictureStarted + darkTime) {
            screenImage.color = Color.Lerp(Color.black, Color.white, (Time.time - timePictureStarted) / darkTime);

            yield return null;
        }
    }
}