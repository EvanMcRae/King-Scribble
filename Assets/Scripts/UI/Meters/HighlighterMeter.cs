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
    [SerializeField] private Image pulse1, pulse2;
    private bool isEmpty;
    private bool pulsing;
    [SerializeField] private GameObject barParent;
    [SerializeField] private float minX, maxX;

    protected override void Start()
    {
        base.Start();
        UpdateMeter(1);
    }

    protected override void UpdateMeter(float percent)
    {
        if (!isEmpty)
        {
            Vector3 pos = barParent.transform.localPosition;
            pos.x = Mathf.Lerp(maxX, minX, percent);
            barParent.transform.localPosition = pos;

            if (percent < 1 && !pulsing)
            {
                anim.SetFloat("Pulse", 1);
                pulse1.enabled = true;
                pulse2.enabled = true;
                pulsing = true;
            }

            if (percent <= 0)
            {
                isEmpty = true;
            }
        }
    }

    protected override void ReleaseCursor()
    {
        if (pulsing)
        {
            anim.SetFloat("Pulse", 0);
            pulse1.enabled = false;
            pulse2.enabled = false;
            pulsing = false;
        }
        
        if (isEmpty)
        {
            Debug.Log("replenish the meter here!!");
        }
    }
}