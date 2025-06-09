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
        if (_linesFolder != null)
            _currentLine = Instantiate(_linePref, mousePos, Quaternion.identity, _linesFolder.transform);
        else
            _currentLine = Instantiate(_linePref, mousePos, Quaternion.identity);
        SetPenParams(_currentLine);
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

    private void SetPenParams(Line line) // // Temporary - will be rewritten with eventual Line.cs refactor
    {   
        line.is_pen = true;
        line.SetThickness(_startThickness);
        line.collisionsActive = false;
        line.GetComponent<LineRenderer>().startColor = _startColor;
        line.GetComponent<LineRenderer>().endColor = _startColor;
        line.startPoint.enabled = true;
        line.startPoint.color = _startColor;
    }
}
