using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScribbleMeter : MonoBehaviour
{
    const int NUM_SPRITES = 10; 
    [SerializeField] private Sprite[] sprites = new Sprite[NUM_SPRITES+1];
    private Image image;

    void Start()
    {
        image = GetComponent<Image>();
        PlayerVars.instance.doodleEvent += UpdateSprite;
    }

    void UpdateSprite(float doodlePercent)
    {
        image.sprite = sprites[Mathf.CeilToInt(doodlePercent * NUM_SPRITES)];
    }

    void OnDestroy()
    {
        PlayerVars.instance.doodleEvent -= UpdateSprite;
    }
}
