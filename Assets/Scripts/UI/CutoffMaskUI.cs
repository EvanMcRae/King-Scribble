using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

// Referenced: https://www.youtube.com/watch?v=rtYCqVahq6A
public class CutoffMaskUI : Image
{
    public override Material materialForRendering
    {
        get
        {
            Material material = new(base.materialForRendering);
            material.SetInt("_StencilComp", (int)CompareFunction.NotEqual);
            return material;
        }
    }
}
