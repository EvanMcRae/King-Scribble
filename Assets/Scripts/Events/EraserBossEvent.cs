using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EraserBossEvent : MonoBehaviour
{
    [SerializeField] private EBInkPipe left;
    [SerializeField] private EBInkPipe right;
    [SerializeField] private GameObject button;
    private static SpriteRenderer buttonTop;
    [SerializeField] static bool isButtonActive = false;
    [SerializeField] private SoundPlayer soundPlayer;
    private int soundsPlaying = 0;

    void Start()
    {
        buttonTop = button.transform.Find("Top").GetComponent<SpriteRenderer>();
        DeactivateButton();
    }

    public static void ActivateButton() {
        // change color to purple
        buttonTop.color = Color.white;
        isButtonActive = true;
    }

    public static void DeactivateButton() {
        // change color to black
        buttonTop.color = Color.black;
        isButtonActive = false;
    }

    public void Activate() {
        if(isButtonActive) {
            left.Activate();
            right.Activate();
            soundsPlaying = 2;
 
        }
    }

    public void Deactivate() {
        if(isButtonActive) {
            left.Deactivate();
            right.Deactivate();
            soundsPlaying = 0;
        }
    }

    public void DeactivateLeft() {
        left.Break();
    }

    public void DeactivateRight() {
        right.Break();
    }

}

