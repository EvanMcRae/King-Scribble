using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Eraser : Tool
{
    [SerializeField] private float _radius = 0.5f;

    protected override void BeginDraw(Vector2 mousePos)
    {
        base.BeginDraw(mousePos);
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
