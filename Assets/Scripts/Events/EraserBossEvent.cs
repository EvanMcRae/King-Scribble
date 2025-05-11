using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EraserBossEvent : MonoBehaviour
{
    [SerializeField] private Transform l_start;
    [SerializeField] private Transform l_active;
    [SerializeField] private Transform l_deactivated;
    [SerializeField] private Transform r_start;
    [SerializeField] private Transform r_active;
    [SerializeField] private Transform r_deactivated;
    [SerializeField] private GameObject l_inkfall;
    [SerializeField] private GameObject r_inkfall;
    [SerializeField] private GameObject button;
    [SerializeField] private SoundPlayer soundPlayer;
    private int soundsPlaying = 0;
    [SerializeField] bool isButtonActive = false; // whether KS should be able to interact with it
    bool isButtonPressed = false; // if it is pressed
    private  SpriteRenderer buttonTop;
    private Color purpleColor;

    void Start()
    {
        l_inkfall.transform.position = l_start.position;
        r_inkfall.transform.position = r_start.position;
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
            l_inkfall.transform.DOMoveY(l_active.position.y, 0.5f);
            r_inkfall.transform.DOMoveY(r_active.position.y, 0.5f);
            soundsPlaying = 2;
            soundPlayer.PlaySound("Ink.Flow", 1, true);
        }
    }

    public void Deactivate() {
        isButtonPressed = false;
        if(isButtonActive) {
            StartCoroutine(Deactivate_());
        }
    }

    public void DeactivateLeft() {
        StartCoroutine(DeactivateLeft_());
    }

    public void DeactivateRight() {
        StartCoroutine(DeactivateRight_());
    }
    

    IEnumerator Deactivate_() {
        l_inkfall.transform.DOMoveY(l_deactivated.position.y, 0.5f);
        r_inkfall.transform.DOMoveY(r_deactivated.position.y, 0.5f);
        yield return new WaitForSeconds(0.5f);
        soundsPlaying = 0;
        soundPlayer.EndSound("Ink.Flow");
        l_inkfall.transform.position = l_start.position;
        r_inkfall.transform.position = r_start.position;
    }

    // Stops ink from falling
    IEnumerator DeactivateLeft_() {
        l_inkfall.transform.DOMoveY(l_deactivated.position.y, 0.5f);
        yield return new WaitForSeconds(0.5f);
        soundsPlaying--;
        if (soundsPlaying == 0) soundPlayer.EndSound("Ink.Flow");
        l_inkfall.SetActive(false);
    }

    IEnumerator DeactivateRight_() {
        r_inkfall.transform.DOMoveY(r_deactivated.position.y, 0.5f);
        yield return new WaitForSeconds(0.5f);
        soundsPlaying--;
        if (soundsPlaying == 0) soundPlayer.EndSound("Ink.Flow");
        r_inkfall.SetActive(false);
    }

    IEnumerator test() {
        for(int i = 0; i < 30; i++) {
            Activate();
            yield return new WaitForSeconds(1);
            Deactivate();
            yield return new WaitForSeconds(1);
        }
    }
}

