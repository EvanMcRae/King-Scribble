﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EraserMeter : MonoBehaviour
{
    const int NUM_SPRITES = 10;
    [SerializeField] private Sprite[] sprites = new Sprite[NUM_SPRITES+1];
    private Image image;
    [SerializeField] private Animator anim;
    private bool isEmpty;

    void Start()
    {
        image = GetComponent<Image>();
        DrawManager.GetTool(ToolType.Eraser)._fuelEvent += UpdateSprite;
        DrawManager.GetTool(ToolType.Eraser)._releaseCursor += ReleaseCursor;
        UpdateSprite(1);
    }

    void UpdateSprite(float eraserPercent)
    {
        image.sprite = sprites[Mathf.FloorToInt(eraserPercent * NUM_SPRITES)];
        if (eraserPercent <= 0)
        {
            isEmpty = true;
            anim.enabled = true;
            anim.SetTrigger("IsEmpty");
            anim.SetBool("IsUsing", true);
        }
    }

    void ReleaseCursor()
    {
        if (isEmpty)
        {
            anim.SetBool("IsUsing", false);
        }   
    }

    void OnDestroy()
    {
        DrawManager.GetTool(ToolType.Eraser)._fuelEvent -= UpdateSprite;
        DrawManager.GetTool(ToolType.Eraser)._releaseCursor -= ReleaseCursor;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            anim.enabled = !anim.enabled;
        }
    }
}