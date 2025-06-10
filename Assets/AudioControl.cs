using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioControl : MonoBehaviour
{
    public AudioSource audioSource;

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            audioSource.Pause();
        }
    }
}
