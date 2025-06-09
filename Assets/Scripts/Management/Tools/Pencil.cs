using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pencil : Tool
{
    [SerializeField] private float _lineThickness;
    [SerializeField] private GameObject _linesFolder;

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
