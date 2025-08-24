using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HighlighterMeter : ToolMeter
{
    public HighlighterMeter()
    {
        _toolType = ToolType.Highlighter;
    }

    const int NUM_SPRITES = 10;
    [SerializeField] private Sprite[] sprites = new Sprite[NUM_SPRITES + 1];
    private bool isEmpty;

    protected override void Start()
    {
        base.Start();
        UpdateMeter(1);
    }

    protected override void UpdateMeter(float percent)
    {
        meter.sprite = sprites[Mathf.FloorToInt(percent * NUM_SPRITES)];
        if (percent <= 0)
        {
            isEmpty = true;
            anim.enabled = true;
            anim.SetTrigger("IsEmpty");
            anim.SetBool("IsUsing", true);
        }
    }

    protected override void ReleaseCursor()
    {
        if (isEmpty)
        {
            anim.SetBool("IsUsing", false);
        }
    }
}