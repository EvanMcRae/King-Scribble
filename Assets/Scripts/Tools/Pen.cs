using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Pen", menuName = "ScriptableObjects/Pen", order = 3)]
public class Pen : LineTool
{
    [Header("Line Thickness")]
    [Tooltip("The thickness of the drawn pen lines before they are completed.")]
    [SerializeField] private float _startThickness;
    [Tooltip("The thickness of the drawn pen lines after they are completed.")]
    [SerializeField] private float _endThickness;

    [Header("Object Fill")]
    [SerializeField] private Material _fillMat;
    [SerializeField] private List<Sprite> _fillTextures = new();
    [SerializeField] private Color _fillColor;

    [SerializeField] private GameObject _trailPref;

    [Tooltip("The layer(s) used for cutting - note that simply adding a layer to this list will NOT automatically make all objects in that layer cuttable - a script on each object is needed.")]
    [SerializeField] private LayerMask _cutLayers;
    [Tooltip("The factor by which to smooth pen lines once they are complete.")]
    [SerializeField] private int _smoothSeverity = 1;

    private GameObject _trail;

    private float _startThicknessF;
    private float _endThicknessF;

    public delegate void UpdatePenAction(float mass);
    public UpdatePenAction _updatePenAreaEvent;

    public override void OnStart()
    {
        base.OnStart();
        _startThicknessF = _startThickness * _thicknessMult;
        _endThicknessF = _endThickness * _thicknessMult;
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

            if (_currentLine.CheckClosedLoop() || _currentLine.hasOverlapped || GetTempFuelRemaining() <= 0f)
            {
                EndDraw();
                _drawCooldown = _DRAW_CD;
            }
        }

        else if (!_currentLine.canDraw && _currentLine.hasDrawn)
        {
            BeginDraw(mousePos);
        }
    }

    public override void EndDraw()
    {
        base.EndDraw();

        if (_currentLine == null)
            return;

        if (_currentLine.CheckClosedLoop())
        {
            CreateObject(_currentLine);
        }

        else
        {
            Destroy(_currentLine.gameObject);
            _currentLine = null;
            ResetTempFuel();
        }

        _currentLine = null;
    }

    public override void RightClick(Vector2 mousePos)
    {
        base.RightClick(mousePos);

        if (_trail == null)
        {
            _trail = Instantiate(_trailPref, mousePos, Quaternion.identity);
        }

        _trail.transform.position = mousePos;
        RaycastHit2D hit = Physics2D.CircleCast(mousePos, 0.1f, Vector2.zero, Mathf.Infinity, _cutLayers);

        if (hit == true)
        {
            hit.collider.gameObject.GetComponent<Breakable>().Break();
            DrawManager.instance.toolSoundPlayer.PlaySound("Player.Slice");
        }
    }

    public override void EndRightClick()
    {
        base.EndRightClick();

        if (_trail != null)
        {
            Destroy(_trail);
            _trail = null;
        }
    }

    public override void SetLineParams(Line line) // // Temporary - will be rewritten with eventual Line.cs refactor
    {
        base.SetLineParams(line);
        line.SetThickness(_startThicknessF);
        line.collisionsActive = false;
        line.GetComponent<LineRenderer>().startColor = _startColor;
        line.GetComponent<LineRenderer>().endColor = _startColor;
        line.startPoint.enabled = true;
        line.startPoint.color = _startColor;
    }

    private void CreateObject(Line line) // Also temporary - should be rewritten with eventual Line.cs refactor as well
    {
        if (!line.AddPolyCollider()) // Add polygon collider, returns false if collision within object found
        {
            ResetTempFuel();
            return;
        }

        line.SmoothPen(_smoothSeverity);
        if (PlayerVars.instance.curCamZoom > 10)  // Reduce unnecessary vertices if the camera is large enough (larger camera = more points drawn)
            line.Generalize(PlayerVars.instance.curCamZoom / 60f, (int)PlayerVars.instance.curCamZoom * 5);
        line.AddPhysics(); // This function also sets the weight of the object based on its area
        line.SetThickness(_endThicknessF); // Set the thickness of the line
        line.SetColor(_endColor); // Set the color of the line
        _updatePenAreaEvent?.Invoke(line.area); // This does something

        // Create material for pen object polygon mesh (texture selected by object area)
        int fillTexture = Mathf.FloorToInt(Mathf.Min(Line.MAX_WEIGHT, line.area) / Line.MAX_WEIGHT * (_fillTextures.Count - 1));
        MaterialPropertyBlock fillMatBlock = new MaterialPropertyBlock();
        fillMatBlock.SetColor("_Color", _fillColor);
        fillMatBlock.SetTexture("_MainTex", _fillTextures[fillTexture].texture);

        DrawManager.instance.toolSoundPlayer.PlaySound("Drawing.PenComplete");

        line.AddMesh(_fillMat, fillMatBlock); // Create a mesh from the polygon collider and assign the set material
    }
}
