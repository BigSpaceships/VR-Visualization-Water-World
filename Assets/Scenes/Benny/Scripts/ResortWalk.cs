using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResortWalk : MonoBehaviour
{
    [SerializeField] AudioClip solidWalk;
    [SerializeField] AudioClip grassWalk;
    [SerializeField] LayerMask groundMask;
    CharacterController charController;
    AudioSource audioSource;
    bool isGrounded = true;
    void Awake()
    {
        charController = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (charController.velocity.sqrMagnitude < 1 || SceneManager.GetSceneByName("R_Area2 Under Water").isLoaded)
        {
            if (!isGrounded) isGrounded = true;
            audioSource.Pause();
            return;
        }
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, 0.6f, groundMask))
        {
            if (hit.transform.gameObject.layer == 14) SwitchClip(solidWalk);
            else SwitchClip(grassWalk);
            if (!isGrounded) isGrounded = true;
        }
        else if (isGrounded)
        {
            isGrounded = false;
            audioSource.Pause();
        }
    }

    void SwitchClip(AudioClip clip)
    {
        if (audioSource.clip != clip)
        {
            audioSource.clip = clip;
            if (clip == solidWalk) audioSource.volume = 0.5f;
            else audioSource.volume = 0.2f;
            audioSource.Play();
            return;
        }
        else if (!audioSource.isPlaying) audioSource.UnPause();
    }

}
