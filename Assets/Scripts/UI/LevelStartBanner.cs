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
    private bool needsStartPos = false;
    
    void Start()
    {
       rectTransform = GetComponent<RectTransform>();
       if (needsStartPos)
            rectTransform.position = new Vector3(rectTransform.position.x, active.position.y);
    }

    public void setStartPosition() {
        Debug.Log("setting banner pos");
        needsStartPos = true;
    }

    public void PlayLevelStartAnimation() {
        Debug.Log("playing banner animation!");
        StartCoroutine(StartAnimation());
        ScreenWipe.PostUnwipe -= PlayLevelStartAnimation;
    }

    // Update is called once per frame
    private IEnumerator StartAnimation() {
        //transform.DOMoveY(active.position., 0.5f);
        yield return new WaitForSeconds(2.0f);
        transform.DOMoveY(end.position.y, 0.8f);
    }
}
