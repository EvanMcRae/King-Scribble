using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PenMeter : MonoBehaviour
{
    private Image image;

    void Start()
    {
        image = GetComponent<Image>();
        PlayerVars.instance.penEvent += UpdateSprite;
    }

    void UpdateSprite(float penPercent)
    {
        image.fillAmount = penPercent;
    }

    void OnDestroy()
    {
        PlayerVars.instance.penEvent -= UpdateSprite;
    }
}
