using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SkiRotation : MonoBehaviour
{

    public Transform ski;
    public Transform newParent;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseUpAsButton() {
        //Set the parent of the ski object
        ski.SetParent(newParent);

        //Adjust the local rotation of the ski object
        ski.localRotation = Quaternion.Euler(new Vector3(0f, 45f, 0f));
    }

}
