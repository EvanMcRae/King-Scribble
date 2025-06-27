using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Highlighter", menuName = "ScriptableObjects/Highlighter", order = 5)]
public class Highlighter : LineTool
{
    [SerializeField] private float _lineThickness;
    [SerializeField] private Material _mat;
    [SerializeField] private GameObject _lightPref;
    private float _lineThicknessF;

    public override void OnStart()
    {
        base.OnStart();
        _lineThicknessF = _lineThickness * _thicknessMult;
    }

    public override void BeginDraw(Vector2 mousePos)
    {
        base.BeginDraw(mousePos);
    }

    public override void Draw(Vector2 mousePos)
    {
        base.Draw(mousePos);
        if (_abort) return;

        if (_currentLine.canDraw || !_currentLine.hasDrawn)
        {
            _currentLine.SetPosition(mousePos);
        }

        else if (!_currentLine.canDraw && _currentLine.hasDrawn)
        {
            BeginDraw(mousePos);
        }
    }

    public override void EndDraw()
    {
        base.EndDraw();
        _currentLine.HighlighterFade();
    }

    public override void RightClick(Vector2 mousePos)
    {
        base.RightClick(mousePos);
    }

    public override void SetLineParams(Line line)
    {
        base.SetLineParams(line);
        line.AddLight(_lightPref);
        line.SetThickness(_lineThicknessF);
        line.collisionsActive = false;
        line.SetHighlighterParams(_mat);
        line.SetColor(_startColor);
    }
}
