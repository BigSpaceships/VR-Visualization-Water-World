using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Experimental.Rendering;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class AdventuringCamera : MonoBehaviour {
    public XRBaseInteractor startedAttachment; 
    public XRInteractionManager xrInteractionManager;
    public InputActionProperty rightXButtonAction;
    public Transform rightController;
    public TextMeshProUGUI hintTakePhotoButton;
    public Transform scooter;

    [Header("Camera Settings")] // 
    [SerializeField] private Camera displayCamera;

    [SerializeField] private RawImage screenImage;

    [SerializeField] private Vector2Int displayResolution;

    [Header("Controls")] //
    public InputActionProperty zoomAction = new InputActionProperty(new InputAction("Zoom"));
    public InputActionProperty takePictureAction = new InputActionProperty(new InputAction("Take Picture"));
    
    private float zoom;
    [SerializeField] private Vector2 zoomRange;
    [SerializeField] private float zoomSpeed;

    [Header("Effect Settings")] //
    [SerializeField] private float darkTime;


    private RenderTexture displayRenderTexture;

    public List<RenderTexture> takenPictures;

    public List<PictureDisplayTile.ObjectPictureInformation> scannedObjects;

    public UnityEvent<PictureDisplayTile.ObjectPictureInformation> onPictureTaken;

    private Transform originalParent; //camera's parent
    private Transform originalParentScooter; //scooter's parent


    private void Start() {
        takenPictures = new List<RenderTexture>();

        displayRenderTexture =
            new RenderTexture(displayResolution.x, displayResolution.y, 8, RenderTextureFormat.ARGB32);

        screenImage.texture = displayRenderTexture;

        displayCamera.targetTexture = displayRenderTexture;

        zoom = displayCamera.fieldOfView;

        //xrInteractionManager.SelectEnter(startedAttachment, GetComponent<XRGrabInteractable>());

        rightXButtonAction.action.performed += ctx => ToggleCamera();
        
        ToggleCamera();
        ToggleCamera();
    }

    private void OnDestroy() {
        rightXButtonAction.action.performed -= ctx => ToggleCamera();
    }

    private void ToggleCamera() {
        if (GamePublicV2.instance.moveMode != MoveMode.UnderWater) return;
        if (GamePublicV2.instance.cameraActive) {
            // 退出拍照模式，恢复到原始父物体
            transform.SetParent(originalParent, false);
            GamePublicV2.instance.cameraActive = false;
            hintTakePhotoButton.text = "Active Scooter";
            takePictureAction.action.performed -= TakePictureAction;
            // ==== SeaScooter ====
            originalParentScooter = scooter.parent;
            scooter.transform.SetParent(rightController, false);
            Vector3 v = new Vector3(0, 0.14f, 0.3f);
            scooter.transform.localPosition = v;
        } else {
            // 切换到 right Controller
            originalParent = transform.parent;
            transform.SetParent(rightController, false);
            Vector3 v = new Vector3(-0.1f, -0f, -0.05f);
            transform.localPosition = v;
            transform.localRotation = Quaternion.Euler(0, -90, -70);
            hintTakePhotoButton.text = "Take Picture";
            GamePublicV2.instance.cameraActive = true;
            takePictureAction.action.performed += TakePictureAction;
            // ==== SeaScooter ====
            scooter.transform.SetParent(originalParentScooter, false);
        }
    }

    private void Update() {
        if (GamePublicV2.instance.cameraActive) {
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
        //
        // foreach (var obj in objectsInPicture) {
        //     var pictureInfo = new PictureDisplayTile.ObjectPictureInformation
        //         { pictureTaken = picture, scannedObject = obj };
        //
        //     scannedObjects.Add(pictureInfo);
        //
        //     onPictureTaken.Invoke(pictureInfo);
        // }

        // if (objectsInPicture.Count == 0) {
            var pictureInfo = new PictureDisplayTile.ObjectPictureInformation
                { pictureTaken = picture, scannedObject = null };

            scannedObjects.Add(pictureInfo);

            onPictureTaken.Invoke(pictureInfo);
        // }
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