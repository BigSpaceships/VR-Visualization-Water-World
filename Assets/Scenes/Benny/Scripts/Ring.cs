using UnityEngine;

public class Ring : MonoBehaviour
{
    [SerializeField] Color passedColor;
    bool passed;
    Material mat;
    Color normalColor;

    void Awake()
    {
        mat = GetComponent<Renderer>().material;
        normalColor = mat.GetColor("_EmissionColor");
    }

    void OnTriggerEnter(Collider other)
    {
        if (!passed && other.gameObject.layer == 2)
        {
            mat.SetColor("_EmissionColor", passedColor);
            Skier.PassedRing();
            passed = true;
        }
    }

    public void Reset()
    {
        mat.SetColor("_EmissionColor", normalColor);
        passed = false;
    }
}
