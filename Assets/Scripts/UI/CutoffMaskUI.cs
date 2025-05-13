using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

// Referenced: https://www.youtube.com/watch?v=rtYCqVahq6A
public class CutoffMaskUI : Image
{
    private Material _customMaterial;

    public override Material materialForRendering
    {
        get
        {
            if (_customMaterial == null)
            {
                _customMaterial = new Material(base.materialForRendering);
                _customMaterial.SetInt("_StencilComp", (int)CompareFunction.NotEqual);
            }
            return _customMaterial;
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        _customMaterial = null;
        SetMaterialDirty();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (_customMaterial != null)
        {
            DestroyImmediate(_customMaterial);
        }
    }
}
