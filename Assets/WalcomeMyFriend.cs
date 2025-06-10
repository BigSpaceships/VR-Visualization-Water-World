using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalcomeMyFriend : MonoBehaviour
{

    AudioSource source;
    Collider soundTrigger;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Awake() {
        source=  GetComponent<AudioSource>();
        soundTrigger= GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider collider) {
        if (collider.gameObject.tag == "Player") {
            source.Play();
        }
    }
}
