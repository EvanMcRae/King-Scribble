using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EraserBossEvent : MonoBehaviour
{
    [SerializeField] private EBInkPipe left;
    [SerializeField] private EBInkPipe right;
    [SerializeField] private GameObject button;
    [SerializeField] private SoundPlayer soundPlayer;
    private int soundsPlaying = 0;
    [SerializeField] bool isButtonActive = false; // whether KS should be able to interact with it
    bool isButtonPressed = false; // if it is pressed
    private  SpriteRenderer buttonTop;
    private Color purpleColor;

    void Start()
    {
        buttonTop = button.transform.Find("Top").GetComponent<SpriteRenderer>();
        purpleColor = buttonTop.color;
        DeactivateButton();
        //StartCoroutine(test());
    }

    public void ActivateButton() {
        // change color to purple
        buttonTop.color = purpleColor;
        isButtonActive = true;
        if(isButtonPressed) {
            Activate();
        }
    }

    public void DeactivateButton() {
        // change color to gray
        buttonTop.color = Color.gray;
        if(isButtonPressed) {
            Deactivate();
        }
        isButtonActive = false;
    }

    public void Activate() {
        isButtonPressed = true;
        if(isButtonActive) {
            left.Activate();
            right.Activate();
            soundsPlaying = 2;
 
        }
    }

    public void Deactivate() {
        isButtonPressed = false;
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

