using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteRandomizer : MonoBehaviour
{
    const int NUM_COLORS = 8;
    public Sprite[] sprites = new Sprite[NUM_COLORS];
    void Awake()
    {
        int randSprite = Random.Range(0, NUM_COLORS - 1);
        gameObject.GetComponent<SpriteRenderer>().sprite = sprites[randSprite];
    }
}
