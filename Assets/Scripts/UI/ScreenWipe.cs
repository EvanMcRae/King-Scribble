using System;
using UnityEngine;
using UnityEngine.UI;

// Controls "wipe" effect that occurs between scene changes
// Referenced: https://www.youtube.com/watch?v=rtYCqVahq6A
public class ScreenWipe : MonoBehaviour
{
    public static Action PostWipe;
    public static Action PostUnwipe;
    public static bool over = false;
    [SerializeField] private Image ScreenBlocker;
    [SerializeField] private SoundPlayer soundPlayer;
    public static ScreenWipe instance;

    public void Awake()
    {
        WipeOut();
        instance = this;
    }

    public void PlayWipeOutSound()
    {
        soundPlayer.PlaySound("Level.WipeOut");
    }

    public void WipeIn()
    {
        soundPlayer.PlaySound("Level.WipeIn");
        over = false;
        ScreenBlocker.gameObject.SetActive(true);
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
        over = true;
        PostUnwipe?.Invoke();
        ScreenBlocker.gameObject.SetActive(false);
    }
}