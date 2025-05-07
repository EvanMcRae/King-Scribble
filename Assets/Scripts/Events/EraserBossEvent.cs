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
    private static SpriteRenderer buttonTop;
    [SerializeField] static bool isButtonActive = false;

    void Start()
    {
        l_inkfall.transform.position = l_start.position;
        r_inkfall.transform.position = r_start.position;
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
            l_inkfall.transform.DOMoveY(l_active.position.y, 0.5f);
            r_inkfall.transform.DOMoveY(r_active.position.y, 0.5f);
        }
    }

    public void Deactivate() {
        if(isButtonActive) {
            StartCoroutine(Deactivate_());
        }
    }

    IEnumerator Deactivate_() {
        l_inkfall.transform.DOMoveY(l_deactivated.position.y, 0.5f);
        r_inkfall.transform.DOMoveY(r_deactivated.position.y, 0.5f);
        yield return new WaitForSeconds(0.5f);
        l_inkfall.transform.position = l_start.position;
        r_inkfall.transform.position = r_start.position;
    }
}

