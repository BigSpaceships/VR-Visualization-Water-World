using UnityEngine;

public class Ring : MonoBehaviour
{
    [SerializeField] Color passedColor;
    bool passed;
    void OnTriggerEnter(Collider other)
    {
        if (!passed && other.gameObject.layer == 2)
        {
            GetComponent<Renderer>().material.SetColor("_EmissionColor", passedColor);
            Skier.PassedRing();
            passed = true;
        }
    }
}
