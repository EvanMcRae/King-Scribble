using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EBInkfallEvent : MonoBehaviour
{
    [SerializeField] private Transform l_start;
    [SerializeField] private Transform l_active;
    [SerializeField] private Transform l_deactivated;
    [SerializeField] private Transform r_start;
    [SerializeField] private Transform r_active;
    [SerializeField] private Transform r_deactivated;
    [SerializeField] private GameObject l_inkfall;
    [SerializeField] private GameObject r_inkfall;

    void Start()
    {
        l_inkfall.transform.position = l_start.position;
        r_inkfall.transform.position = r_start.position;
    }

    public void Activate() {
        l_inkfall.transform.DOMoveY(l_active.position.y, 0.5f);
        r_inkfall.transform.DOMoveY(r_active.position.y, 0.5f);
    }

    public void Deactivate() {
        StartCoroutine(Deactivate_());
    }

    IEnumerator Deactivate_() {
        l_inkfall.transform.DOMoveY(l_deactivated.position.y, 0.5f);
        r_inkfall.transform.DOMoveY(r_deactivated.position.y, 0.5f);
        yield return new WaitForSeconds(0.5f);
        l_inkfall.transform.position = l_start.position;
        r_inkfall.transform.position = r_start.position;
    }
}

