using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PictureDisplay : MonoBehaviour {
    public InputActionProperty showDisplayAction = new InputActionProperty(new InputAction("Show Display"));

    [Header("Screens")] //
    public GameObject displayPanelScreen;

    public GameObject pictureObjectListScreen;
    public GameObject objectDisplayScreen;

    public GameObject objectListScreen;
    public GameObject pictureScreen;

    [Header("Object List")] //
    public Transform pictureContainer;

    public GameObject picturePrefab;
    
    [Header("Picture List")] //
    public Transform pictureListContainer;
    
    public GameObject pictureListPrefab;

    [Header("Object Display")] //
    public Transform objectDisplayContainer;

    public GameObject objectDisplayPrefab;
    public TextMeshProUGUI objectDisplayTextName;

    private List<PictureDisplayTile.ObjectPictureInformation> _pictures = new();

    private Dictionary<ScanableObject, List<PictureDisplayTile.ObjectPictureInformation>> _picturesByObject = new();

    private void Start() {
        for (int i = 0; i < pictureContainer.childCount; i++) {
            Destroy(pictureContainer.GetChild(0).gameObject);
        }

        SetScreenForPictureList();
    }

    private void OnEnable() {
        showDisplayAction.action.performed += ToggleVisibility;
    }

    private void OnDisable() {
        showDisplayAction.action.performed -= ToggleVisibility;
    }

    public void ToggleVisibility(InputAction.CallbackContext context) {
        displayPanelScreen.SetActive(!displayPanelScreen.activeSelf);
    }

    public void PictureTaken(PictureDisplayTile.ObjectPictureInformation picture) {
        _pictures.Add(picture);

        var pictureDisplayTile = Instantiate(pictureListPrefab, pictureListContainer).GetComponent<PictureDisplayTile>();
        
        pictureDisplayTile.SetInformation(picture, false);

        if (picture.scannedObject == null) {
            return;
        }

        if (!_picturesByObject.ContainsKey(picture.scannedObject)) {
            var newGameObject = Instantiate(picturePrefab, pictureContainer);

            var displayTile = newGameObject.GetComponent<PictureDisplayTile>();

            displayTile.SetInformation(picture, true, SetScreenForObjectDisplay);

            _picturesByObject[picture.scannedObject] = new List<PictureDisplayTile.ObjectPictureInformation>();
        }

        _picturesByObject[picture.scannedObject].Add(picture);
    }

    public void SetScreenForObjectDisplay(ScanableObject selectedObject) {
        pictureObjectListScreen.SetActive(false);
        objectDisplayScreen.SetActive(true);

        for (int i = 0; i < objectDisplayContainer.childCount; i++) {
            Destroy(objectDisplayContainer.GetChild(0).gameObject);
        }

        for (int i = 0; i < _picturesByObject[selectedObject].Count; i++) {
            var newDisplay = Instantiate(objectDisplayPrefab, objectDisplayContainer)
                .GetComponent<PictureDisplayTile>();

            newDisplay.SetInformation(_picturesByObject[selectedObject][i], false);
        }

        objectDisplayTextName.text = selectedObject.name;
    }

    public void SetScreenForPictureList() {
        pictureObjectListScreen.SetActive(true);
        objectDisplayScreen.SetActive(false);
    }

    public void SetScreenForObjectPictureList() {
        SetScreenForPictureList();

        objectListScreen.SetActive(true);
        pictureScreen.SetActive(false);
    }

    public void SetScreenForPictures() {
        SetScreenForPictureList();

        objectListScreen.SetActive(false);
        pictureScreen.SetActive(true);
    }
}