using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Eraser : Tool
{
    [SerializeField] private float _radius = 0.5f;
    private Vector2 _lastMousePos;

    protected override void BeginDraw(Vector2 mousePos)
    {
        base.BeginDraw(mousePos);
        mousePos += new Vector2(0.5f, -0.5f);
        _lastMousePos = mousePos;
        EraserFunctions.Erase(mousePos, _radius, true);
        return;
    }

    protected override void Draw(Vector2 mousePos)
    {
        base.Draw(mousePos);
    }

    protected override void EndDraw()
    {
        base.EndDraw();
    }
}
