using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PenMeter : MonoBehaviour
{
    const int NUM_SPRITES = 10; 
    [SerializeField] private Sprite[] sprites = new Sprite[NUM_SPRITES+1];
    private Image image;

    void Start()
    {
        image = GetComponent<Image>();
        PlayerController.instance.GetComponent<PlayerVars>().penEvent += UpdateSprite;
    }

    void UpdateSprite(float penPercent)
    {
        image.sprite = sprites[Mathf.CeilToInt(penPercent * NUM_SPRITES)];
    }
}
