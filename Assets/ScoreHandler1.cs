using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreHandler1 : MonoBehaviour {
    // The counter to keep track of the number of collisions
    private static int collisionCount = 0;
    public TextMeshProUGUI CountText;

    // The other GameObject we want to detect collisions with
    //public GameObject GameObject;

    private void Start() {
        Debug.Log("Game start");
    }

    void OnCollisionEnter(Collision collision) {
        // Check if the collided object is the specified targetGameObject
        //string tagName = collision.gameObject.tag;
        //if (tagName == "fruit") {
        //    collisionCount++;
        //} else if (tagName == "orange") {
        //    collisionCount++;
        //}
        //Debug.Log("Collision event executed");
        //if (collision.gameObject.CompareTag("fruit")) {          
        //    collisionCount++;
        //    CountText.text = "FRUIT: " + collisionCount;
        //    Debug.Log("Collision Count: " + collisionCount);
        //}
        //Debug.Log("Collision event executed" + collision.gameObject.tag);
    }

    private void OnTriggerEnter(Collider other) {
        //Debug.Log("Triggered");
        if (other.gameObject.CompareTag("basketball")) {
            collisionCount++;
            //Debug.Log("Triggered, count +1");
            CountText.text = "SCORE: " + collisionCount;
        }
    }


}