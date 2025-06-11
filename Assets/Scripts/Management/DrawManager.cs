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
    Eraser
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

    private Vector2 lastMousePos;

    public SoundPlayer toolSoundPlayer, drawSoundPlayer;
    public SoundPlayer penSoundPlayer;

    private Coroutine currentSoundPause, currentSoundUnpause;
    private float soundPauseCounter = 0, soundPauseThreshold = 0.5f;
    private bool soundPaused = false;

    private void Awake()
    {
        _currentTool = PlayerVars.instance.inventory._toolUnlocks[(int)PlayerVars.instance.cur_tool - 1];
        instance = this;
        if (PlayerVars.instance != null && !HUDButtonCursorHandler.inside)
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

        _currentTool = PlayerVars.instance.inventory._toolUnlocks[(int)PlayerVars.instance.cur_tool - 1];

        // Player dead -> stop drawing and return
        if (PlayerVars.instance.isDead)
        {
            // End all sounds
            if (currentSoundPause != null) { AudioManager.instance.StopCoroutine(currentSoundPause); }
            if (currentSoundUnpause != null) { AudioManager.instance.StopCoroutine(currentSoundUnpause); }
            drawSoundPlayer.EndAllSounds();
            // Stop drawing
            _currentTool.EndDraw();
            return;
        }

        // Drawing CD active -> decrement and return
        if (_currentTool._drawCooldown > 0)
        {
            _currentTool._drawCooldown -= Time.deltaTime;
            return;
        }

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Overlap player -> stop drawing
        if (_currentTool._drawing && PlayerController.instance.OverlapsPosition(mousePos))
        {
            // End all sounds
            if (currentSoundPause != null) { AudioManager.instance.StopCoroutine(currentSoundPause); }
            if (currentSoundUnpause != null) { AudioManager.instance.StopCoroutine(currentSoundUnpause); }
            drawSoundPlayer.EndAllSounds();
            // Stop drawing
            _currentTool.EndDraw();
        }

        // LMB down -> start drawing
        if ((Input.GetMouseButtonDown(0) || (Input.GetMouseButton(0) && !_currentTool._beganDraw)) && !_currentTool._rmbActive)
        {
            _currentTool.BeginDraw(mousePos);
            drawSoundPlayer.PlaySound(_currentTool._sound, 1, true);
        }

        // LMB held -> draw
        if (Input.GetMouseButton(0) && _currentTool._beganDraw && !_currentTool._rmbActive)
        {
            _currentTool.Draw(mousePos);
            SoundPauseCheck(mousePos);
        }

        // LMB released -> stop drawing
        if (Input.GetMouseButtonUp(0) && _currentTool._beganDraw)
        {
            // End all sounds
            if (currentSoundPause != null) { AudioManager.instance.StopCoroutine(currentSoundPause); }
            if (currentSoundUnpause != null) { AudioManager.instance.StopCoroutine(currentSoundUnpause); }
            drawSoundPlayer.EndAllSounds();
            // Stop drawing
            _currentTool.EndDraw();
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
            if (Input.GetKeyDown("1"))
            {
                SwitchTool(0);
            }
            if (Input.GetKeyDown("2"))
            {
                SwitchTool(1);
            }
            if (Input.GetKeyDown("3"))
            {
                SwitchTool(2);
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

    // Kept for reference - will be removed upon move to Eraser.cs
    private IEnumerator EraseMarch(Vector2 mouse_pos)
    {
        Vector2 marchPos = lastMousePos;
        int ct = 0, interval = 3;
        do
        {
            marchPos = Vector2.MoveTowards(marchPos, mouse_pos, RESOLUTION);
            EraserFunctions.Erase(marchPos, eraserRadius, true);
            ct++;
            if (ct % interval == 0) yield return new WaitForEndOfFrame();
        } while (Vector2.Distance(marchPos, mouse_pos) > RESOLUTION);
        EraserFunctions.Erase(mouse_pos, eraserRadius, true);
        lastMousePos = mouse_pos;
    }

    // Kept for reference - will be removed upon move to Eraser.cs
    private void Draw(Vector2 mouse_pos)
    {
        // Handle eraser first so it's exempt from overlap checks
        if (PlayerVars.instance.cur_tool == ToolType.Eraser)
        {
            mouse_pos += new Vector2(0.5f, -0.5f);
            SoundPauseCheck(mouse_pos);
            if (PlayerVars.instance.eraserFuelLeft() > 0)
            {
                // March along line from previous to current eraser pos if it's too far away
                if (Vector2.Distance(mouse_pos, lastMousePos) > RESOLUTION)
                {
                    StartCoroutine(EraseMarch(mouse_pos));
                }
                else
                {
                    EraserFunctions.Erase(mouse_pos, eraserRadius, true);
                    lastMousePos = mouse_pos;
                }
            }
            // else EndDraw()
            return;
        }


    }

    private void SoundPauseCheck(Vector2 mouse_pos)
    {
        if (Vector2.Distance(lastMousePos, mouse_pos) < 0.01f)
        {
            if (currentSoundUnpause != null)
                AudioManager.instance.StopCoroutine(currentSoundUnpause);
            soundPauseCounter += Time.deltaTime;
            if (soundPauseCounter >= soundPauseThreshold && !soundPaused)
            {
                currentSoundPause = AudioManager.instance.StartCoroutine(AudioManager.instance.FadeAudioSource(drawSoundPlayer.sources[0], 0.2f, 0f, () => { }));
                soundPaused = true;
            }
        }
        else if (soundPaused)
        {
            if (currentSoundPause != null)
                AudioManager.instance.StopCoroutine(currentSoundPause);
            soundPauseCounter = 0;
            soundPaused = false;
            currentSoundUnpause = AudioManager.instance.StartCoroutine(AudioManager.instance.FadeAudioSource(drawSoundPlayer.sources[0], 0.2f, 1f, () => { }));
        }
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
        // If LMB held, stop
        if (_currentTool._drawing)
            _currentTool.EndDraw();
        // If RMB held, stop
        if (_currentTool._rmbActive)
            _currentTool.EndRightClick();
        LoadSubmeter(newTool);
        if (!HUDButtonCursorHandler.inside)
        {
            SetCursor();
        }
        ToolIndicator.instance.UpdateMenu(newTool);
        PlayerVars.instance.cur_tool = newTool;
    }

    public bool IsUsingTool()
    {
        return _currentTool._drawing || _currentTool._rmbActive;
    }
}



