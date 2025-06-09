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
        if (_linesFolder != null)
            _currentLine = Instantiate(_linePref, mousePos, Quaternion.identity, _linesFolder.transform);
        else
            _currentLine = Instantiate(_linePref, mousePos, Quaternion.identity);
        SetPencilParams(_currentLine);
    }

    protected override void Draw(Vector2 mousePos)
    {
        base.Draw(mousePos);
        if (_currentLine.canDraw || !_currentLine.hasDrawn) 
            _currentLine.SetPosition(mousePos);
        else if (!_currentLine.canDraw && _currentLine.hasDrawn)
            BeginDraw(mousePos);
    }

    protected override void EndDraw()
    {
        base.EndDraw();
    }

    private void SetPencilParams(Line line) // Temporary - will be rewritten with eventual Line.cs refactor
    {
        line.is_pen = false;
        line.SetThickness(_lineThickness);
        line.collisionsActive = true;
        line.GetComponent<LineRenderer>().startColor = _startColor;
        line.GetComponent<LineRenderer>().endColor = _endColor;
        line.gameObject.layer = LayerMask.NameToLayer("Lines");
    }
}
