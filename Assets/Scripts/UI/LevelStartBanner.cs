using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class LevelStartBanner : MonoBehaviour
{

    [SerializeField] private RectTransform start;
    [SerializeField] private RectTransform active;
    [SerializeField] private RectTransform end;
    RectTransform rectTransform;
    
    void Start()
    {
       rectTransform = GetComponent<RectTransform>();
    }

    public void setStartPosition() {
        Debug.Log("setting banner pos");
        rectTransform = GetComponent<RectTransform>();
        rectTransform.position = new Vector3(active.position.x, rectTransform.position.y); 
    }

    public void PlayLevelStartAnimation() {
        Debug.Log("playing banner animation!");
        StartCoroutine(StartAnimation());
        ScreenWipe.PostUnwipe -= PlayLevelStartAnimation;
    }

    // Update is called once per frame
    private IEnumerator StartAnimation() {
        //transform.DOMoveX(active.position.x, 0.5f);
        yield return new WaitForSeconds(2.0f);
        transform.DOMoveX(end.position.x, 0.8f);
    }
}
