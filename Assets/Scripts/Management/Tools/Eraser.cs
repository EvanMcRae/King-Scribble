using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Eraser", menuName = "ScriptableObjects/Eraser", order = 4)]
public class Eraser : Tool
{
    [SerializeField] private float _radius = 0.5f;

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

        mousePos += new Vector2(0.5f, -0.5f);
        if (_curFuel > 0)
        {
            // March along line from previous to current eraser pos if it's too far away
            if (Vector2.Distance(mousePos, _lastMousePos) > _RESOLUTION)
            {
                // Unfortunate side effect of the refactor is needing to perform this inside of a
                // singleton Monobehavior (i.e. DrawManager) since it requires a Coroutine.
                // TODO in the future -- optimize erasing to the point where it no longer needs delay
                DrawManager.StartMarchDrawing(mousePos);
            }
            else
            {
                EraserFunctions.Erase(mousePos, _radius, true);
                ResyncMousePos(mousePos);
            }
        }

        // EndDraw only happens when the user releases their cursor

        return;
    }

    public override void MarchStepDraw(Vector2 marchPos)
    {
        EraserFunctions.Erase(marchPos, _radius, true);
    }

    public override void EndDraw()
    {
        base.EndDraw();
    }
}
