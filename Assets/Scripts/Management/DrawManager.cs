using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum ToolType
{
    None,
    Pencil,
    Pen,
    Eraser,
    Highlighter
}

// Referenced: https://www.youtube.com/watch?v=SmAwege_im8
public class DrawManager : MonoBehaviour
{
    public Tool _currentTool;
    [SerializeField] public float eraserRadius = 0.5f; // Remove once erase fully implemented in Eraser.cs
    public const float RESOLUTION = 0.1f; // RF
    private ToolType activeSubmeter = ToolType.Pencil;

    public Texture2D defaultCursor; // The texture file for the cursor used by default

    [SerializeField] private List<GameObject> submeters;

    public static DrawManager instance;

    public SoundPlayer toolSoundPlayer, drawSoundPlayer;
    public SoundPlayer penSoundPlayer;

    private Coroutine currentSoundPause, currentSoundUnpause;
    private float soundPauseCounter = 0, soundPauseThreshold = 0.5f;
    private bool soundPaused = false;

    [SerializeField] private ToolDatabase _toolDatabase;

    [Tooltip("Tool line width will be multiplied by this value for this stage.")]
    public float _lineWidthMult = 1;

    private void Awake()
    {
        instance = this;

        foreach (Tool tool in _toolDatabase.tools)
        {
            if (tool is LineTool lineTool)
            {
                lineTool.SetThicknessMult(_lineWidthMult);
            }
        }

        _currentTool = null;
        if (PlayerVars.instance != null)
        {
            if (PlayerVars.instance.inventory.hasTool(PlayerVars.instance.cur_tool))
            {
                _currentTool = GetTool(PlayerVars.instance.cur_tool);
            }
            else
            {
                PlayerVars.instance.cur_tool = ToolType.None; // TODO fallback, shouldn't trigger
            }
        }    

        if (PlayerVars.instance != null && !HUDButtonCursorHandler.inside && _currentTool != null)
            SwitchTool(PlayerVars.instance.cur_tool);
        else
            SetCursor();
    }

    void LoadSubmeter(ToolType tool)
    {
        if (submeters[(int)activeSubmeter] != null)
        {
            // TODO fancier animation here or something, for now this will have to do :(
            submeters[(int)activeSubmeter].GetComponent<Canvas>().enabled = false;
            activeSubmeter = tool;
            submeters[(int)activeSubmeter].GetComponent<Canvas>().enabled = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        instance = this;
        if (PlayerVars.instance == null) return;
        if (_currentTool == null) return;

        // Player dead -> stop drawing and return
        if (PlayerVars.instance.isDead)
        {
            // End all sounds
            ResetDrawSoundPause();
            if (_currentTool._drawing)
            {
                drawSoundPlayer.EndAllSounds();
                _currentTool.EndDraw();
            }
            return;
        }

        // Drawing CD active -> decrement and return
        if (_currentTool._drawCooldown > 0)
        {
            _currentTool._drawCooldown -= Time.deltaTime;
            if (Input.GetMouseButtonUp(0))
            {
                _currentTool._releaseCursor?.Invoke();
            }
            return;
        }

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Overlap player -> stop drawing
        if (_currentTool._drawing && _currentTool._stopsOnPlayer && PlayerController.instance.OverlapsPosition(mousePos))
        {
            // Stop drawing
            _currentTool.EndDraw();
            ResetDrawSoundPause();
        }

        // LMB down -> start drawing
        if ((Input.GetMouseButtonDown(0) || (Input.GetMouseButton(0) && !_currentTool._beganDraw)) && !_currentTool._rmbActive)
        {
            _currentTool.BeginDraw(mousePos);
        }

        // LMB held -> draw
        if (Input.GetMouseButton(0) && _currentTool._beganDraw && !_currentTool._rmbActive)
        {
            _currentTool.Draw(mousePos);
            SoundPauseCheck(mousePos);
        }

        // LMB released -> stop drawing
        if (Input.GetMouseButtonUp(0))
        {
            if (_currentTool._drawing)
            {
                // Stop drawing
                _currentTool.EndDraw();
            }
            ResetDrawSoundPause();
            _currentTool._releaseCursor?.Invoke();
        }

        // RMB down -> start rmb action
        if (Input.GetMouseButtonDown(1) && !_currentTool._beganDraw)
        {
            _currentTool.RightClick(mousePos);
        }

        // RMB held -> rmb action
        if (Input.GetMouseButton(1) && _currentTool._rmbActive)
        {
            _currentTool.RightClick(mousePos);
        }

        // RMB released -> stop rmb action
        if (Input.GetMouseButtonUp(1) && _currentTool._rmbActive)
        {
            _currentTool.EndRightClick();
        }

        if (GameManager.paused) return;

        // Tool selection
        int count = PlayerVars.instance.inventory._toolTypes.Count;
        if (count > 0)
        {
            // Tool switching
            if (Input.GetKeyDown("1") || Input.GetKeyDown(KeyCode.Keypad1))
            {
                SwitchTool(0);
            }
            if (Input.GetKeyDown("2") || Input.GetKeyDown(KeyCode.Keypad2))
            {
                SwitchTool(1);
            }
            if (Input.GetKeyDown("3") || Input.GetKeyDown(KeyCode.Keypad3))
            {
                SwitchTool(2);
            }
            if (Input.GetKeyDown("4") || Input.GetKeyDown(KeyCode.Keypad4))
            {
                SwitchTool(3);
            }
            // Tool scrolling
                float scrollDelta;
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                scrollDelta = Input.mouseScrollDelta.x;
            else
                scrollDelta = Input.mouseScrollDelta.y;

            int index = PlayerVars.instance.inventory._toolTypes.IndexOf(PlayerVars.instance.cur_tool);
            if (scrollDelta >= 0.1f)
            {
                index = (index - 1 + count) % count;
                SwitchTool(index);
            }
            else if (scrollDelta <= -0.1f)
            {
                index = (index + 1 + count) % count;
                SwitchTool(index);
            }
        }
    }

    private void SoundPauseCheck(Vector2 mouse_pos)
    {
        if (Vector2.Distance(_currentTool.GetLastMousePos(), mouse_pos) < 0.01f)
        {
            soundPauseCounter += Time.deltaTime;
            if (soundPauseCounter >= soundPauseThreshold && !soundPaused)
            {
                if (currentSoundPause != null)
                    AudioManager.instance.StopCoroutine(currentSoundPause);
                if (currentSoundUnpause != null)
                    AudioManager.instance.StopCoroutine(currentSoundUnpause);

                currentSoundPause = AudioManager.instance.StartCoroutine(AudioManager.instance.FadeAudioSource(drawSoundPlayer.sources[0], 0.2f, 0f, () => { }));
                soundPaused = true;
            }
        }
        else if (soundPaused)
        {
            if (currentSoundPause != null)
                AudioManager.instance.StopCoroutine(currentSoundPause);
            if (currentSoundUnpause != null)
                AudioManager.instance.StopCoroutine(currentSoundUnpause);
            soundPauseCounter = 0;
            soundPaused = false;
            currentSoundUnpause = AudioManager.instance.StartCoroutine(AudioManager.instance.FadeAudioSource(drawSoundPlayer.sources[0], 0.2f, 1f, () => { }));
        }

        _currentTool.ResyncMousePos(mouse_pos);
    }

    public void ResetDrawSoundPause()
    {
        soundPaused = false;
        if (drawSoundPlayer.sources.Length > 1 && drawSoundPlayer.sources[0] != null)
            drawSoundPlayer.sources[0].volume = 1;
        if (currentSoundPause != null)
            AudioManager.instance.StopCoroutine(currentSoundPause);
        if (currentSoundUnpause != null)
            AudioManager.instance.StopCoroutine(currentSoundUnpause);
        soundPauseCounter = 0;
    }

    public void SetCursor(bool _override = false)
    {
        Texture2D texture;
        if (_currentTool && !_override)
            texture = _currentTool._cursor;
        else
            texture = defaultCursor;
        Cursor.SetCursor(texture, Vector2.zero, CursorMode.Auto);
    }

    public void SwitchTool(int index)
    {
        if (PlayerVars.instance.inventory._toolTypes.Count > index)
        {
            ToolType newTool = PlayerVars.instance.inventory._toolTypes[index];
            TrySwitchTool(newTool);
        }
    }

    public void TrySwitchTool(ToolType newTool)
    {
        if (PlayerVars.instance.cur_tool != newTool)
        {
            SwitchTool(newTool);
            toolSoundPlayer.PlaySound("Player.SelectTool");
        }
    }

    public void SwitchTool(ToolType newTool)
    {
        if (_currentTool != null)
        {
            // If LMB held, stop
            if (_currentTool._drawing)
            {
                _currentTool.EndDraw();
                _currentTool._releaseCursor?.Invoke(); // kinda needs to be done, unfortunately
            }
            ResetDrawSoundPause();
            // If RMB held, stop
            if (_currentTool._rmbActive)
                _currentTool.EndRightClick();
        }
        
        LoadSubmeter(newTool);
        PlayerVars.instance.cur_tool = newTool;
        _currentTool = GetTool(newTool);
        if (!HUDButtonCursorHandler.inside)
        {
            SetCursor();
        }
        ToolIndicator.instance.UpdateMenu(newTool);
    }

    public bool IsUsingTool()
    {
        if (_currentTool == null) return false;
        return _currentTool._drawing || _currentTool._rmbActive;
    }

    public static Tool GetTool(ToolType toolType)
    {
        return instance._toolDatabase.tools[(int)toolType - 1];
    }

    public static void StartMarchDrawing(Vector2 mousePos)
    {
        instance.StartCoroutine(instance.MarchDrawing(mousePos));
    }

    private IEnumerator MarchDrawing(Vector2 mousePos)
    {
        Vector2 marchPos = _currentTool.GetLastMousePos();
        _currentTool.ResyncMousePos(mousePos);
        int ct = 0, interval = _currentTool._marchInterval;
        do
        {
            marchPos = Vector2.MoveTowards(marchPos, mousePos, Tool._RESOLUTION);
            _currentTool.MarchStepDraw(marchPos);
            ct++;
            if (ct % interval == 0) yield return new WaitForEndOfFrame();
        } while (Vector2.Distance(marchPos, mousePos) > Tool._RESOLUTION);
        _currentTool.MarchStepDraw(marchPos);
    }

    public void StopDrawSounds()
    {
        if (currentSoundPause != null) { AudioManager.instance.StopCoroutine(currentSoundPause); }
        if (currentSoundUnpause != null) { AudioManager.instance.StopCoroutine(currentSoundUnpause); }
        drawSoundPlayer.EndAllSounds();
    }
}



