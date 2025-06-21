using UnityEngine;

public class Ring : MonoBehaviour
{
    [SerializeField] Color passedColor;
    [SerializeField] AudioClip passClip;
    [SerializeField] float ringVolume;
    bool passed;
    Material mat;
    Color normalColor;
    public static AudioSource effectSource;

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
            effectSource.PlayOneShot(passClip, ringVolume);
            passed = true;
        }
    }

    public void Reset()
    {
        mat.SetColor("_EmissionColor", normalColor);
        passed = false;
    }
}
