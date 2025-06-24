using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalcomeMyFriend : MonoBehaviour
{

    AudioSource source;

    private void Awake() {
        source=  GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider collider) {
        if (collider.gameObject.tag == "Player") {
            source.Play();
        }
    }
}
