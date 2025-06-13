using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonScreen : MonoBehaviour
{
    Animator animator;
    int presses;
    void Awake()
    {
        gameObject.SetActive(false);
        animator = GetComponent<Animator>();
    }
    public void ButtonPress()
    {
        switch (presses)
        {
            case 0:
                gameObject.SetActive(true);
                animator.SetTrigger("FadeIn");
                presses = 1;
                break;
            case 1:
                animator.SetTrigger("FadeOut");
                presses = 2;
                break;
            case 2:
                animator.SetTrigger("FadeIn");
                presses = 1;
                break;
        }
    }
}
