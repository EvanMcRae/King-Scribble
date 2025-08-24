using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PenMeter : ToolMeter
{
    public PenMeter()
    {
        _toolType = ToolType.Pen;
    }

    protected override void UpdateMeter(float percent)
    {
        meter.fillAmount = percent;
    }

    protected override void UpdateMonitor(float percent)
    {
        monitor.fillAmount = percent;
    }
}
