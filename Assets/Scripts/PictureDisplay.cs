using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PictureDisplay : MonoBehaviour {
    [Header("Screens")] public GameObject pictureObjectListScreen;
    public GameObject objectDisplayScreen;

    [Header("List")] public Transform pictureContainer;
    public GameObject picturePrefab;

    [Header("Object Display")] public Transform objectDisplayContainer;
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

    public void PictureTaken(PictureDisplayTile.ObjectPictureInformation picture) {
        _pictures.Add(picture);

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
}