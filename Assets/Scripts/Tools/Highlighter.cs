using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Highlighter", menuName = "ScriptableObjects/Highlighter", order = 5)]
public class Highlighter : LineTool
{
    [SerializeField] private float _lineThickness;
    [SerializeField] private Material _mat;
    [SerializeField] private GameObject _LMBLightPref;
    [SerializeField] private GameObject _RMBLightPref;
    [SerializeField] private float _rmbLightSpeed = 1f;
    [SerializeField] private float _rmbLightDuration = 5f;
    [SerializeField] private float _rmbLightForwardOffset = 1f;
    [SerializeField] private float _rmbCooldown = 0.5f;
    [SerializeField] private float _duration = 5f;
    [SerializeField] private float _LMBLightRadius = 0.1f;
    private float _lineThicknessF;
    private bool _rmbSpawned;
    [SerializeField] private int _lightThreshold;

    public override void OnStart()
    {
        base.OnStart();
        _lineThicknessF = _lineThickness * _thicknessMult;
        _rmbSpawned = false;
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
        _currentLine.HighlighterFade(_duration);
    }

    public override void RightClick(Vector2 mousePos)
    {
        base.RightClick(mousePos);
        if (_rmbActive && !_rmbSpawned)
        {
            _rmbSpawned = true;
            _drawCooldown += _rmbCooldown;
            Vector2 tempPos = new(PlayerVars.instance.transform.position.x + _rmbLightForwardOffset, PlayerVars.instance.transform.position.y);
            GameObject temp = Instantiate(_RMBLightPref, tempPos, Quaternion.identity);
            HighlighterRMBLight temp_l = temp.GetComponent<HighlighterRMBLight>();
            temp_l._destroyEvent += RMBLightDestroy;
            temp_l.SetSpeed(_rmbLightSpeed);
            temp_l.SetTarget(mousePos);
            temp_l.SetDuration(_rmbLightDuration);
            temp_l.StartMove();
        }
        _rmbActive = false;
    }

    public override void SetLineParams(Line line)
    {
        base.SetLineParams(line);
        line.SetHLThreshold(_lightThreshold);
        line.AddLight(_LMBLightPref, 0);
        line.SetThickness(_lineThicknessF);
        line.SetHLRadius(_LMBLightRadius);
        line.collisionsActive = false;
        line.SetHighlighterParams(_mat);
        line.SetColor(_startColor);
    }

    private void RMBLightDestroy()
    {
        _rmbSpawned = false;
    }
}
