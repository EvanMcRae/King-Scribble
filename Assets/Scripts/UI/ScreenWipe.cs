using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Controls "wipe" effect that occurs between scene changes
// Referenced: https://www.youtube.com/watch?v=rtYCqVahq6A
public class ScreenWipe : MonoBehaviour
{
    public Action PostWipe;
    public Action PostUnwipe;
    public static bool over = false;
    [SerializeField] private Image ScreenBlocker;

    public void Awake()
    {
        WipeOut();
    }

    public void WipeIn()
    {
        over = false;
        ScreenBlocker.raycastTarget = true;
        GetComponent<Animator>().SetTrigger("WipeIn");
    }

    public void WipeOut()
    {
        GetComponent<Animator>().SetTrigger("WipeOut");
    }

    public void CallPostWipe()
    {
        PostWipe?.Invoke();
    }

    public void ScreenRevealed()
    {
        PostUnwipe?.Invoke();
        Invoke("PostCooldown", 0.1f);
    }

    public void PostCooldown()
    {
        over = true;
        ScreenBlocker.raycastTarget = false;
    }
}