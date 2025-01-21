
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PictureDisplayTile : MonoBehaviour {
    public TextMeshProUGUI nameText;
    public RawImage imageRenderer;

    public ObjectPictureInformation info;
    
    public UnityEvent<ScanableObject> onPictureSelected;

    public void SetInformation(ObjectPictureInformation info, bool showName, UnityAction<ScanableObject> pictureSelectedAction = null) {
        this.info = info;

        if (showName) {
            nameText.text = info.scannedObject.name;
        }

        imageRenderer.texture = info.pictureTaken;
        
        nameText.transform.parent.gameObject.SetActive(showName);

        if (pictureSelectedAction != null) {
            onPictureSelected = new UnityEvent<ScanableObject>();
            onPictureSelected.AddListener(pictureSelectedAction);
        }
    }
    
    [Serializable]
    public struct ObjectPictureInformation {
        public ScanableObject scannedObject;
        public RenderTexture pictureTaken;
    }

    public void OnClicked() {
        onPictureSelected?.Invoke(info.scannedObject);
    }
}


