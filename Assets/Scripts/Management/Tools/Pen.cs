using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pen : Tool
{
    [SerializeField] private float _startThickness;
    [SerializeField] private float _endThickness;
    [SerializeField] private Material _fillMat;
    [SerializeField] private GameObject _trailPref;
    [SerializeField] private GameObject _linesFolder;
    [SerializeField] private List<Sprite> _fillTextures = new();

    private GameObject _trail;
    private bool _cutting = false;

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

    protected override void RightClick(Vector2 mousePos)
    {
        base.RightClick(mousePos);
    }
}
