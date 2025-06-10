using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Eraser", menuName = "ScriptableObjects/Eraser", order = 4)]
public class Eraser : Tool
{
    [SerializeField] private float _radius = 0.5f;
    private Vector2 _lastMousePos;

    public new const int _index = 3;

    protected override void BeginDraw(Vector2 mousePos)
    {
        base.BeginDraw(mousePos);
        mousePos += new Vector2(0.5f, -0.5f);
        _lastMousePos = mousePos;
        EraserFunctions.Erase(mousePos, _radius, true);
        return;
    }

    protected override void Draw(Vector2 mousePos)
    {
        base.Draw(mousePos);
        // TODO: Port eraser "draw" functionality here (rewrite to avoid using coroutine)
    }
}
