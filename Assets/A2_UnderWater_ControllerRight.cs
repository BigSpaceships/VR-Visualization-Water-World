using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class A2_UnderWater_ControllerRight : MonoBehaviour {
    public SeaScooter seaScooterFunction;
    public AdventuringCamera digitalCameraFunction;
    [SerializeField] public InputActionReference activeScooderTakePicButton;
    [SerializeField] public InputActionReference switchScooderCameraButton;

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
    }
}
