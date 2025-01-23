using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateGrass : MonoBehaviour
{

    private void OnTriggerEnter(Collider other) {
        // Check if the object entering the trigger is tagged as "Grass"
        if (other.CompareTag("Grass")) {
            // Enable the grass renderer to make it visible
            Renderer grassRenderer = other.GetComponent<Renderer>();
            if (grassRenderer != null) {
                grassRenderer.enabled = true;
            }
        }
    }

    private void OnTriggerExit(Collider other) {
        // Check if the object exiting the trigger is tagged as "Grass"
        if (other.CompareTag("Grass")) {
            // Disable the grass renderer to hide it
            Renderer grassRenderer = other.GetComponent<Renderer>();
            if (grassRenderer != null) {
                grassRenderer.enabled = false;
            }
        }
    }
}
