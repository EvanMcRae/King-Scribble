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
    [SerializeField] bool isButtonActive = false; // whether KS should be able to interact with it
    bool isButtonPressed = false; // if it is pressed
    private SpriteRenderer buttonTop;
    private Color purpleColor;
    [SerializeField] private GameObject buttonParticles;
    [SerializeField] private BoxCollider2D buttonBound;
    [SerializeField] private Vector2 offset1, size1, offset2, size2;

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
        buttonParticles.SetActive(true);
        isButtonActive = true;
        if(isButtonPressed) {
            Activate();
        }
    }

    public void DeactivateButton() {
        // change color to gray
        buttonTop.color = Color.gray;
        buttonParticles.SetActive(false);
        if (isButtonPressed) {
            Deactivate();
        }
        isButtonActive = false;
    }

    public void Activate() {
        isButtonPressed = true;
        buttonBound.offset = offset2;
        buttonBound.size = size2;
        if (isButtonActive) {
            soundPlayer.PlaySound("EraserBoss.ButtonActivate");
            left.Activate();
            right.Activate();
        }
    }

    public void Deactivate() {
        isButtonPressed = false;
        buttonBound.offset = offset1;
        buttonBound.size = size1;
        if (isButtonActive) {
            left.Deactivate();
            right.Deactivate();
        }
    }

    public void DeactivateLeft() {
        left.Break();
    }

    public void DeactivateRight() {
        right.Break();
    }
}

