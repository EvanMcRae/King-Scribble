using UnityEngine;
using System.Collections;

public class LineTool : Tool
{
    public Line _linePref;
    [SerializeField] protected Color _startColor;
    [SerializeField] protected Color _endColor;

    public float _thicknessMult = 1;

    protected Transform _linesFolder;
    protected Line _currentLine;

    public override void BeginDraw(Vector2 mousePos)
    {
        base.BeginDraw(mousePos);
        if (_linesFolder == null)
            _linesFolder = new GameObject(GetType().ToString() + "LinesFolder").transform;
        if (_abort) return;
        _currentLine = Instantiate(_linePref, mousePos, Quaternion.identity, _linesFolder.transform);
        SetLineParams(_currentLine);
    }

    public virtual void SetLineParams(Line line)
    {
        line._curTool = this;
    }

    public virtual void SetThicknessMult(float newThicknessMult)
    {
        _thicknessMult = newThicknessMult;
    }

    public Transform GetLinesFolder()
    {
        return _linesFolder;
    }

    public void SwapColors(Line line)
    {
        Color temp = line.GetComponent<LineRenderer>().startColor;
        line.GetComponent<LineRenderer>().startColor = _endColor;
        line.GetComponent<LineRenderer>().endColor = temp;
    }

    public void CheckRefreshLine(Line line)
    {
        if (line == _currentLine)
            EndDraw();
    }
}
