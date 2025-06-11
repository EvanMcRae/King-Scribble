using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Eraser", menuName = "ScriptableObjects/Eraser", order = 4)]
public class Eraser : Tool
{
    [SerializeField] private float _radius = 0.5f;
    private Vector2 _lastMousePos;
    public delegate void ReleaseEraser();
    public ReleaseEraser _releaseEraser;
    public new const int _index = 3;

    public override void BeginDraw(Vector2 mousePos)
    {
        base.BeginDraw(mousePos);
        if (_abort) return;
        mousePos += new Vector2(0.5f, -0.5f);
        _lastMousePos = mousePos;
        EraserFunctions.Erase(mousePos, _radius, true);
        return;
    }

    public override void Draw(Vector2 mousePos)
    {
        base.Draw(mousePos);
        if (_abort) return;
        // TODO: Port eraser "draw" functionality here (rewrite to avoid using coroutine)
    }

    public override void EndDraw()
    {
        base.EndDraw();
        _releaseEraser();
    }

    public void Replenish()
    {
        _curFuel = _maxFuel;
        _fuelEvent(1);
    }
}
