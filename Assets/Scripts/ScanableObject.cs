using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScanableObject : MonoBehaviour {
    public Collider collider;

    public string name;

    public Canvas billboard;
    
    public Camera targetCamera;
    private Vector3 unTransformedMin;
    private Vector3 unTransformedMax;

    public ScanableObject(Vector3 unTransformedMin) {
        this.unTransformedMin = unTransformedMin;
    }

    public LineRenderer lineRenderer;
    public Vector3[] TransformedPoints;
    private protected Vector3[] Points;
    public Vector3 transformedMax;
    public Vector3 transformedMin;

    public float outlineBorderSpacing;

    private void Update() {
        billboard.transform.rotation = targetCamera.transform.rotation;
        
        CalculateBillboardBounds();
    }

    public void CalculateBillboardBounds() {
        var bounds = collider.bounds;

        var boundsMin = bounds.min;
        var boundsMax = bounds.max;

        Points = new Vector3[] {
            new Vector3(boundsMin.x, boundsMin.y, boundsMin.z),
            new Vector3(boundsMin.x, boundsMin.y, boundsMax.z),
            new Vector3(boundsMin.x, boundsMax.y, boundsMin.z),
            new Vector3(boundsMin.x, boundsMax.y, boundsMax.z),
            new Vector3(boundsMax.x, boundsMin.y, boundsMin.z),
            new Vector3(boundsMax.x, boundsMin.y, boundsMax.z),
            new Vector3(boundsMax.x, boundsMax.y, boundsMin.z),
            new Vector3(boundsMax.x, boundsMax.y, boundsMax.z),
        };
        
        TransformedPoints = new Vector3[Points.Length];

        for (int i = 0; i < Points.Length; i++) {
            var point = Points[i];
            
            TransformedPoints[i] = targetCamera.WorldToScreenPoint(point);
        }
        
        transformedMin = TransformedPoints.Aggregate(Vector3.positiveInfinity, Vector3.Min);
        transformedMax = TransformedPoints.Aggregate(Vector3.negativeInfinity, Vector3.Max);

        var transformedCenter = targetCamera.WorldToScreenPoint(bounds.center);

        transformedMin.z = transformedCenter.z;
        transformedMax.z = transformedCenter.z;

        transformedMin -= new Vector3(outlineBorderSpacing, outlineBorderSpacing, 0);
        transformedMax += new Vector3(outlineBorderSpacing, outlineBorderSpacing, 0);

        unTransformedMin = targetCamera.ScreenToWorldPoint(transformedMin);
        unTransformedMax = targetCamera.ScreenToWorldPoint(transformedMax);

        var cameraMin = Vector3.ProjectOnPlane(unTransformedMin, targetCamera.transform.forward);
        var cameraMax = Vector3.ProjectOnPlane(unTransformedMax, targetCamera.transform.forward);
        
        Debug.Log(cameraMin + ", " + cameraMax);

        billboard.transform.position = (unTransformedMin + unTransformedMax) / 2;

        billboard.GetComponent<RectTransform>().sizeDelta =
            new Vector2(cameraMax.x - cameraMin.x, cameraMax.y - cameraMin.y);
        
        // billboard.transform.localScale = new Vector3((cameraMax.x - cameraMin.x) / 2, (cameraMax.y - cameraMin.y) / 2, 1);
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;

        foreach (var point in Points) {
            Gizmos.DrawSphere(point, 0.1f);
        }
        
        Gizmos.color = Color.green;

        foreach (var point in TransformedPoints) {
            Gizmos.DrawSphere(point, 2f);
        }
        
        Gizmos.color = Color.blue;
        
        Gizmos.DrawSphere(transformedMin, 3f);
        Gizmos.DrawSphere(transformedMax, 3f);
        
        Gizmos.color = Color.yellow;
        
        Gizmos.DrawSphere(unTransformedMin, .3f);
        Gizmos.DrawSphere(unTransformedMax, .3f);
    }
}