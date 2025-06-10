using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movementSound : MonoBehaviour
{

    public AudioSource walkingSound;
    private Vector3 lastPosition;
    private float movementThreshold = 0.01f;

    // Start is called before the first frame update
    void Start()
    {
        lastPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(transform.position, lastPosition) > movementThreshold) {
            walkingSound.Play();
            Debug.Log("iswalking");
        } else {
            walkingSound.Stop();
            Debug.Log("isNotWalking");
        }
        lastPosition = transform.position;
    }
}
