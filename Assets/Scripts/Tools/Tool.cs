using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Tool", menuName = "ScriptableObjects/Tool", order = 1)]
public class Tool : ScriptableObject
{
    public ToolType _type;

    public Texture2D _cursor;
    [SerializeField] protected int _maxFuel = 1000;
    [SerializeField] protected SoundClip _sound;

    [Tooltip("Layers that will prevent drawing with this tool. Will take precedence over refill layers - be sure to exclude those here.")]
    [SerializeField] protected LayerMask _noDraw;
    [Tooltip("Layers that will refill the tool's meter when interacted with. Will be overridden by noRefill layers - be sure to exclude these from noRefill.")]
    [SerializeField] protected LayerMask _refill;
    [Tooltip("Layers that will disable refilling the tool's meter when interacted with.")]
    [SerializeField] protected LayerMask _noRefill;
    [Tooltip("The rate, per frame, at which the tool's meter will be refilled when interacting with refill layers.")]
    [SerializeField] protected int _refillRate = 10;

    public bool _drawing = false;
    public bool _beganDraw = false;
    public float _drawCooldown = 0f;
    public bool _rmbActive = false;
    public int _marchInterval = 3;
    public bool _stopsOnPlayer = true;

    protected Vector2 _lastMousePos;

    public const float _RESOLUTION = 0.1f;
    public const float _DRAW_CD = 0.5f;

    protected int _curFuel;
    protected int _tempFuel;
    protected bool _abort;

    public int GetCurFuel() { return _curFuel; }
    public float GetFuelRemaining() { return (float)_curFuel / _maxFuel; }
    public float GetTempFuelRemaining() { return (float)_tempFuel / _maxFuel; }
    public bool GetIsEmpty() { return _curFuel == 0; }
    public bool GetTempEmpty() { return _tempFuel == 0; }

    public delegate void FuelEvent(float fuelPercent);
    public FuelEvent _fuelEvent;
    public delegate void TempFuelEvent(float tempFuelPercent);
    public TempFuelEvent _tempFuelEvent;
    public delegate void ReleaseCursor();
    public ReleaseCursor _releaseCursor;

    void OnEnable()
    {
        OnStart();
    }

    public virtual void OnStart()
    {
        _curFuel = _maxFuel;
        _tempFuel = _curFuel;
        _drawing = false;
        _beganDraw = false;
        _drawCooldown = 0f;
        _rmbActive = false;
    }

    public virtual void BeginDraw(Vector2 mousePos)
    {
        _abort = false;

        // Check base requirements - player not dead, game not paused, etc.
        if (PlayerVars.instance.isDead || !GameManager.canMove || GameManager.paused || _rmbActive || HUDButtonCursorHandler.inside)
        {
            _abort = true;
            return;
        }

        // Check fuel
        if (GetIsEmpty() || GetTempEmpty())
        {
            _abort = true;
            return;
        }

        // Check for refill layers
        RaycastHit2D refillHit = Physics2D.CircleCast(mousePos, 0.1f, Vector2.zero, Mathf.Infinity, _refill);
        RaycastHit2D noRefillHit = Physics2D.CircleCast(mousePos, 0.1f, Vector2.zero, Mathf.Infinity, _noRefill);
        if (refillHit.collider != null && noRefillHit.collider == null)
        {
            AddFuel(_refillRate);
            EndDraw();
            _abort = true;
            return;
        }

        // Check for noDraw layers
        RaycastHit2D noDrawHit = Physics2D.CircleCast(mousePos, 0.1f, Vector2.zero, Mathf.Infinity, _noDraw); // Note - _noDraw may be empty for tools that can be used anywhere
        if (noDrawHit.collider != null)
        {
            _abort = true;
            return;
        }

        _beganDraw = true;
        _drawing = true;
        DrawManager.instance.drawSoundPlayer.PlaySound(_sound, 1, true);
    }

    public virtual void Draw(Vector2 mousePos)
    {
        _abort = false;

        if (PlayerVars.instance.isDead || !GameManager.canMove || GameManager.paused || _rmbActive || HUDButtonCursorHandler.inside)
        {
            _drawCooldown = _DRAW_CD;
            _abort = true;
            return;
        }

        // Check fuel
        if (GetIsEmpty() || GetTempEmpty())
        {
            EndDraw();
            _drawCooldown = _DRAW_CD;
            _abort = true;
            return;
        }

        // Check for refill layers
        RaycastHit2D refillHit = Physics2D.CircleCast(mousePos, 0.1f, Vector2.zero, Mathf.Infinity, _refill);
        RaycastHit2D noRefillHit = Physics2D.CircleCast(mousePos, 0.1f, Vector2.zero, Mathf.Infinity, _noRefill);
        if (refillHit.collider != null && noRefillHit.collider == null)
        {
            AddFuel(_refillRate);
            EndDraw();
            _abort = true;
            return;
        }

        // Check for noDraw layers
        RaycastHit2D noDrawHit = Physics2D.CircleCast(mousePos, 0.1f, Vector2.zero, Mathf.Infinity, _noDraw);
        if (noDrawHit.collider != null)
        {
            _drawCooldown = _DRAW_CD;
            EndDraw();
            _abort = true;
            return;
        }

    }

    // To be called during general march routine from DrawManager
    public virtual void MarchStepDraw(Vector2 marchPos)
    {
    }

    public virtual void EndDraw()
    {
        _beganDraw = false;
        _drawing = false;
        DrawManager.instance.StopDrawSounds();
    }

    public virtual void RightClick(Vector2 mousePos)
    {
        if (PlayerVars.instance.isDead || !GameManager.canMove || GameManager.paused || _beganDraw || HUDButtonCursorHandler.inside) { return; }
        _rmbActive = true;
    }

    public virtual void EndRightClick()
    {
        _rmbActive = false;
    }

    public virtual void SpendFuel(int amount)
    {
        if (PlayerVars.instance.cheatMode) return;
        _curFuel = (int)Mathf.Clamp(_curFuel - amount, 0f, _maxFuel);
        _tempFuel = _curFuel;
        _fuelEvent?.Invoke(GetFuelRemaining());
    }

    public virtual void AddFuel(int amount)
    {
        _curFuel = (int)Mathf.Clamp(_curFuel + amount, 0f, _maxFuel);
        _tempFuel = _curFuel;
        _fuelEvent?.Invoke(GetFuelRemaining());
        _tempFuelEvent?.Invoke(GetTempFuelRemaining());
    }

    public virtual void MaxFuel()
    {
        _curFuel = _maxFuel;
        _tempFuel = _curFuel;
        _fuelEvent?.Invoke(GetFuelRemaining());
        _tempFuelEvent?.Invoke(GetTempFuelRemaining());
    }

    public virtual void SpendTempFuel(int amount)
    {
        if (PlayerVars.instance.cheatMode) return;
        _tempFuel = (int)Mathf.Clamp(_tempFuel - amount, 0f, _maxFuel);
        _tempFuelEvent?.Invoke(GetTempFuelRemaining());
    }

    public virtual void ResetTempFuel()
    {
        if (PlayerVars.instance.cheatMode) return;
        _tempFuel = _curFuel;
        _tempFuelEvent?.Invoke(GetTempFuelRemaining());
    }

    public Vector2 GetLastMousePos()
    {
        return _lastMousePos;
    }

    public void ResyncMousePos(Vector2 mousePos)
    {
        _lastMousePos = mousePos;
    }
}
