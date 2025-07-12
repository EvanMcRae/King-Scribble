using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScribbleMeter : ToolMeter
{
    public ScribbleMeter()
    {
        _toolType = ToolType.Pencil;
    }

    protected override void UpdateMeter(float percent)
    {
        anim.SetFloat("Fullness", percent);
    }
}
