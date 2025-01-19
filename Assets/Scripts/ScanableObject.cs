using System.Linq;
using TMPro;
using UnityEngine;

public class ScanableObject : MonoBehaviour {
    public Collider collider;

    public string name;

    public Canvas billboard;
    
    public Camera targetCamera;

    public LineRenderer lineRenderer;

    public float outlineBorderSpacing;

    [SerializeField] private TextMeshProUGUI nameText;

    private void Update() {
        billboard.transform.rotation = targetCamera.transform.rotation;
        
        CalculateBillboardBounds();
        
        nameText.text = name;
    }

    public void CalculateBillboardBounds() {
        var bounds = collider.bounds;

        var boundsMin = bounds.min;
        var boundsMax = bounds.max;

        var points = new Vector3[] {
            new Vector3(boundsMin.x, boundsMin.y, boundsMin.z),
            new Vector3(boundsMin.x, boundsMin.y, boundsMax.z),
            new Vector3(boundsMin.x, boundsMax.y, boundsMin.z),
            new Vector3(boundsMin.x, boundsMax.y, boundsMax.z),
            new Vector3(boundsMax.x, boundsMin.y, boundsMin.z),
            new Vector3(boundsMax.x, boundsMin.y, boundsMax.z),
            new Vector3(boundsMax.x, boundsMax.y, boundsMin.z),
            new Vector3(boundsMax.x, boundsMax.y, boundsMax.z),
        };
        
        var transformedPoints = new Vector3[points.Length];

        for (int i = 0; i < points.Length; i++) {
            var point = points[i];
            
            transformedPoints[i] = targetCamera.WorldToScreenPoint(point);
        }
        
        var transformedMin = transformedPoints.Aggregate(Vector3.positiveInfinity, Vector3.Min);
        var transformedMax = transformedPoints.Aggregate(Vector3.negativeInfinity, Vector3.Max);

        var transformedCenter = targetCamera.WorldToScreenPoint(bounds.center);

        transformedMin.z = transformedCenter.z;
        transformedMax.z = transformedCenter.z;

        transformedMin -= new Vector3(outlineBorderSpacing, outlineBorderSpacing, 0);
        transformedMax += new Vector3(outlineBorderSpacing, outlineBorderSpacing, 0);

        var unTransformedMin = targetCamera.ScreenToWorldPoint(transformedMin);
        var unTransformedMax = targetCamera.ScreenToWorldPoint(transformedMax);

        var cameraMin = Vector3.ProjectOnPlane(unTransformedMin, targetCamera.transform.forward);
        var cameraMax = Vector3.ProjectOnPlane(unTransformedMax, targetCamera.transform.forward);

        billboard.transform.position = (unTransformedMin + unTransformedMax) / 2;

        billboard.GetComponent<RectTransform>().sizeDelta =
            new Vector2(cameraMax.x - cameraMin.x, cameraMax.y - cameraMin.y);
    }
}