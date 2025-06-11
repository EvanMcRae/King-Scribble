using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Pencil", menuName = "ScriptableObjects/Pencil", order = 2)]
public class Pencil : Tool
{
    [SerializeField] private float _lineThickness;
    [SerializeField] private GameObject _linesFolder;
    [Tooltip("The factor by which to smooth pencil lines once they are complete.")]
    [SerializeField] private int _smoothSeverity = 3;

    public new const int _index = 1;

    public override void BeginDraw(Vector2 mousePos)
    {
        base.BeginDraw(mousePos);
        if (_abort) return;
        if (_linesFolder != null)
        {
            _currentLine = Instantiate(_linePref, mousePos, Quaternion.identity, _linesFolder.transform);
        }

        else
        {
            _currentLine = Instantiate(_linePref, mousePos, Quaternion.identity);
        }
        SetPencilParams(_currentLine);
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

        if (_currentLine != null)
        {
            if (_currentLine.GetPointsCount() < 2) Destroy(_currentLine.gameObject);
            _currentLine.SmoothPencil(_smoothSeverity);
        }

        _currentLine = null;
    }
    // NOTE: SET TO PRIVATE ONCE ALL THE ERASER SHIT IS FIGURED OUT
    public void SetPencilParams(Line line) // Temporary - will be rewritten with eventual Line.cs refactor
    { // Also I know it sucks for this to be public - temporarily needed in order for EraserFunctions to work
        line.is_pen = false;
        line.SetThickness(_lineThickness);
        line.collisionsActive = true;
        line.GetComponent<LineRenderer>().startColor = _startColor;
        line.GetComponent<LineRenderer>().endColor = _endColor;
        line.gameObject.layer = LayerMask.NameToLayer("Lines");
    }

    public override void SpendFuel(int amount)
    {
        base.SpendFuel(amount);
        PlayerController.instance.ResizePlayer(GetFuelRemaining());
        if (_curFuel == 0 && !PlayerVars.instance.isDead)
        {
            PlayerVars.instance.isDead = true;
            PlayerController.instance.DeathSound();
            PlayerVars.instance.DieEvent();
        }
    }
}
