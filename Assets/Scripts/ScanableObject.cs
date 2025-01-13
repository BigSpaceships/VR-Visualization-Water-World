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

    private void Update() {
        billboard.transform.rotation = targetCamera.transform.rotation;
        
        CalculateBillboardBounds();
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
        var transformedMax = transformedPoints.Aggregate(Vector3.positiveInfinity, Vector3.Min);

        unTransformedMin = targetCamera.ScreenToWorldPoint(transformedMin);
        unTransformedMax = targetCamera.ScreenToWorldPoint(transformedMax);
        
        lineRenderer.SetPositions(new Vector3[] { transformedMin, transformedMax });
    }
}