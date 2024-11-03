
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor(typeof(AdventuringCamera))]
public class AdventuringCameraEditor : Editor {
    public override VisualElement CreateInspectorGUI() {
        var baseElement = new VisualElement();

        InspectorElement.FillDefaultInspector(baseElement, serializedObject, this);

        var pictureButton = new Button { text = "Take Picture" };

        pictureButton.clicked += () => {
            ((AdventuringCamera)target).TakePicture();
        };

        baseElement.Add(pictureButton);

        return baseElement;
    }
}
