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
    [SerializeField] public Line linePrefab;
    [SerializeField] public float eraserRadius = 0.5f; // radius of the raycast of what will be erased
    [SerializeField] private GameObject PencilLinesFolder;
    public const float RESOLUTION = 0.1f;
    public const float DRAW_CD = 0.5f;
    private Line currentLine;
    private float drawCooldown = 0f;
    private ToolType activeSubmeter = ToolType.Pencil;
    public bool isDrawing = false; // True when the mouse is being held down with an drawing tool
    public bool isErasing = false; // True when the mouse is being held down with an erasing tool

    public Color pencilColor_start; // Color of pencil lines at the start of the gradient
    public Color pencilColor_end; // Color of pencil lines at the end of the gradient
    public Color penColor_start; // Color of pen lines while being drawn
    public Color penColor_fin; // Color of pen lines once finished
    public float pencilThickness; // Thickness of pencil lines
    public float penThickness_start; // Thickness of pen lines while being drawn
    public float penThickness_fin; // Thickness of pen lines once finished

    public Texture2D defaultCursor; // The texture file for the cursor used by default
    public Texture2D pencilCursor; // The texture file for the cursor used for the pencil
    public Texture2D penCursor; // The texture file for the cursor used for the pen
    public Texture2D eraserCursor; // The texture file for the cursor used for the eraser

    public Material fillMat; // The material to fill pen objects with (temporary)
    public Sprite fillTexture; // The texture to fill pen objects with (temporary)
    public Color fillColor; // The color to fill pen objects with (temporary)
    private MaterialPropertyBlock fillMatBlock; // Material property overrides for pen fill (temporary)

    private float scrollThreshold;

    [SerializeField] private List<GameObject> submeters;

    public static DrawManager instance;

    private Vector2 lastMousePos;
    private bool beganDraw = false;

    [SerializeField] private SoundPlayer soundPlayer;
    [SerializeField] private List<SoundClip> drawSounds = new();
    private Coroutine currentSoundPause, currentSoundUnpause;
    private float soundPauseCounter = 0, soundPauseThreshold = 0.5f;
    private bool soundPaused = false;

    private void Awake()
    {
        instance = this;

        SetCursor(PlayerVars.instance.cur_tool);
        LoadSubmeter(PlayerVars.instance.cur_tool);

        fillMatBlock = new MaterialPropertyBlock();
        fillMatBlock.SetTexture("_MainTex", fillTexture.texture);
        fillMatBlock.SetColor("_Color", fillColor);
    }

    void LoadSubmeter(ToolType tool)
    {
        // TODO fancier animation here or something, for now this will have to do :(
        submeters[(int)activeSubmeter].GetComponent<Canvas>().enabled = false;
        activeSubmeter = tool;
        submeters[(int)activeSubmeter].GetComponent<Canvas>().enabled = true;
    }
    
    // Update is called once per frame
    void Update()
    {
        // Can't draw if you're dead/paused
        if (PlayerVars.instance.isDead) {
            EndDraw();
            currentLine = null;
            return;
        }

        // If the drawing cooldown is active, decrement it and don't do anything
        if (drawCooldown > 0) { 
            drawCooldown -= Time.deltaTime;
            return;
        }

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (isDrawing && PlayerController.instance.OverlapsPosition(mousePos))
        {
            EndDraw();
            currentLine = null;
        }

        // If the mouse has just been pressed, start drawing
        if (Input.GetMouseButtonDown(0) || (Input.GetMouseButton(0) && !beganDraw) && GameManager.canMove && !PlayerVars.instance.isDead && !GameManager.paused)
        {
            beganDraw = true;
            switch (PlayerVars.instance.cur_tool)
            {
                case ToolType.Pencil:
                    if (PlayerVars.instance.doodleFuelLeft() > 0) BeginDraw(mousePos);
                    break;
                case ToolType.Pen:
                    if (PlayerVars.instance.penFuelLeft() > 0) BeginDraw(mousePos);
                    break;
                case ToolType.Eraser:
                    if (PlayerVars.instance.eraserFuelLeft() > 0) BeginDraw(mousePos);
                    break;
                case ToolType.None:
                    beganDraw = false;
                    break;
            }
        }
        
        // If the mouse is continuously held, continue to draw
        if (Input.GetMouseButton(0) && beganDraw && GameManager.canMove && !PlayerVars.instance.isDead && !GameManager.paused)
            Draw(mousePos);

        // If the mouse has been released, stop drawing
        if (beganDraw && (Input.GetMouseButtonUp(0) || !GameManager.canMove || PlayerVars.instance.isDead || GameManager.paused))
        {
            beganDraw = false;
            EndDraw();
        }

        // Eraser meter cooldown reset
        if (Input.GetMouseButtonUp(0) && PlayerVars.instance.cur_tool == ToolType.Eraser)
        {
            PlayerVars.instance.releaseEraser?.Invoke();
        }

        if (GameManager.paused) return;

        // [1] key pressed - switch to pencil
        if (Input.GetKeyDown("1") && PlayerVars.instance.inventory.hasTool(ToolType.Pencil) && PlayerVars.instance.cur_tool != ToolType.Pencil)
        {
            LoadSubmeter(ToolType.Pencil);
            if (isDrawing) // checking for if something has interrupted the drawing process while the mouse button is being held down
				EndDraw();
			PlayerVars.instance.cur_tool = ToolType.Pencil;
            Cursor.SetCursor(pencilCursor, Vector2.zero, CursorMode.ForceSoftware);
            ToolIndicator.instance.UpdateMenu(PlayerVars.instance.cur_tool);
        }

        // [2] key pressed - switch to pen
        if (Input.GetKeyDown("2") && PlayerVars.instance.inventory.hasTool(ToolType.Pen) && PlayerVars.instance.cur_tool != ToolType.Pen)
        {
            LoadSubmeter(ToolType.Pen);
            if (isDrawing) // checking for if something has interrupted the drawing process while the mouse button is being held down
				EndDraw();
			PlayerVars.instance.cur_tool = ToolType.Pen;
            Cursor.SetCursor(penCursor, Vector2.zero, CursorMode.ForceSoftware);
            ToolIndicator.instance.UpdateMenu(PlayerVars.instance.cur_tool);
        }

        // [3] key pressed - switch to eraser
        if (Input.GetKeyDown("3") && PlayerVars.instance.inventory.hasTool(ToolType.Eraser) && PlayerVars.instance.cur_tool != ToolType.Eraser)
        { 
            LoadSubmeter(ToolType.Eraser);
            if (isDrawing) // checking for if something has interrupted the drawing process while the mouse button is being held down
				EndDraw();
			PlayerVars.instance.cur_tool = ToolType.Eraser;
            Cursor.SetCursor(eraserCursor, Vector2.zero, CursorMode.ForceSoftware);
            ToolIndicator.instance.UpdateMenu(PlayerVars.instance.cur_tool);
        }
    }

    private void BeginDraw(Vector2 mouse_pos)
    {
        if (PlayerVars.instance.cur_tool == ToolType.Eraser)
        {
            mouse_pos += new Vector2(0.5f, -0.5f);
            lastMousePos = mouse_pos;
            EraserFunctions.Erase(mouse_pos, eraserRadius, true);
            soundPlayer.PlaySound(drawSounds[(int)ToolType.Eraser], 1, true);
            return;
        }

        // Don't draw if our cursor overlaps the ground, the "no draw" layer, the "pen lines" layer, the "objects" layer, or the "player" layer (3, 6, 7, 9, and 10 respectively)
        int layerMask = (1 << 3) | (1 << 6) | (1 << 7) | (1 << 9) | (1 << 10);
        RaycastHit2D hit = Physics2D.CircleCast(mouse_pos, 0.1f, Vector2.zero, Mathf.Infinity, layerMask);
        if (hit.collider != null)
        {
            beganDraw = false;
            return;
        }
        layerMask = (1 << 4); // If our cursor overlaps the "water" layer, prevent drawing - and if the pen is selected, slowly refill the meter
        hit = Physics2D.CircleCast(mouse_pos, 0.1f, Vector2.zero, Mathf.Infinity, layerMask);
        if (hit.collider != null)
        {
            beganDraw = false;
            if (PlayerVars.instance.cur_tool == ToolType.Pen && PlayerVars.instance.penFuelLeft() != 1f)
                PlayerVars.instance.AddPenFuel(10);
            return;
        }
		isDrawing = true; // the user is drawing
        if (PlayerVars.instance.cur_tool == ToolType.Pencil) {
            if(PencilLinesFolder != null) {
            currentLine = Instantiate(linePrefab, mouse_pos, Quaternion.identity, PencilLinesFolder.transform); // Create a new line with the first point at the mouse's current position
            }
            else {
                currentLine = Instantiate(linePrefab, mouse_pos, Quaternion.identity);
            }
            SetPencilParams(currentLine);
        }
        else if (PlayerVars.instance.cur_tool == ToolType.Pen) {
            currentLine = Instantiate(linePrefab, mouse_pos, Quaternion.identity); // Create a new line with the first point at the mouse's current position
            currentLine.is_pen = true;
            currentLine.SetThickness(penThickness_start);
            currentLine.collisionsActive = false;
            currentLine.GetComponent<LineRenderer>().startColor = penColor_start;
            currentLine.GetComponent<LineRenderer>().endColor = penColor_start;
        }
        soundPlayer.PlaySound(drawSounds[(int)PlayerVars.instance.cur_tool], 1, true);
    }

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

            else EndDraw();
            return;
        }

        // Stop drawing if our cursor overlaps the ground, the "no draw" layer, the "pen lines" layer, or the "objects" layer (3, 6, 7, and 9 respectively)
        int layerMask = (1 << 3) | (1 << 6) | (1 << 7) | (1 << 9);
        RaycastHit2D hit = Physics2D.CircleCast(mouse_pos, 0.1f, Vector2.zero, Mathf.Infinity, layerMask);
        if (hit.collider != null) {
            EndDraw();
            drawCooldown = DRAW_CD;
            return;
        }
        layerMask = (1 << 4); // If our cursor overlaps the "water" layer, prevent drawing - and if the pen is selected, slowly refill the meter
        hit = Physics2D.CircleCast(mouse_pos, 0.1f, Vector2.zero, Mathf.Infinity, layerMask);
        if (hit.collider != null)
        {
            EndDraw();
            if (PlayerVars.instance.cur_tool == ToolType.Pen && PlayerVars.instance.penFuelLeft() != 1f)
                PlayerVars.instance.AddPenFuel(10);
            else
                drawCooldown = DRAW_CD;
            return;
        }

        if (currentLine == null) return; // Why would this even be needed

        if (currentLine.canDraw || !currentLine.hasDrawn) { // If the line can draw, create a new point at the mouse's current position
            currentLine.SetPosition(mouse_pos);

            if (PlayerVars.instance.cur_tool == ToolType.Pen) // If we are drawing with a pen, check for a closed loop
            {
                if (currentLine.CheckClosedLoop() || currentLine.hasOverlapped || PlayerVars.instance.tempPenFuelLeft() <= 0) // If a closed loop or collision is created: end the line, enable physics, and start a short cooldown
                {
                    EndDraw(); // Enabling physics will take place in this function through a second (admittedly redundant) closed loop check
                    drawCooldown = DRAW_CD; // Set a short cooldown (to prevent accidentally drawing a new line immediately after)
                }
            }  
        }

        else if (!currentLine.canDraw && currentLine.hasDrawn) // If the line was stopped by attempting to draw over an unavailable area, continue when available
            BeginDraw(mouse_pos);

        SoundPauseCheck(mouse_pos);
        lastMousePos = mouse_pos;
    }

    private void EndDraw()
    {
        isDrawing = false; // the user has stopped drawing
        beganDraw = false;
        if (currentSoundPause != null)
            AudioManager.instance.StopCoroutine(currentSoundPause);
        if (currentSoundUnpause != null)
            AudioManager.instance.StopCoroutine(currentSoundUnpause);
        soundPlayer.EndAllSounds();

        if (currentLine != null)
        {
            if (currentLine.GetPointsCount() < 2) // Destroy the current line if it is too small
                Destroy(currentLine.gameObject);

            if (PlayerVars.instance.cur_tool == ToolType.Pen) // If we are drawing with a pen, check for a closed loop
            {
                if (currentLine.CheckClosedLoop()) // If the line is a closed loop: enable physics, set width and color to final parameters, and set weight based on area of the drawn polygon
                {
                    if (!currentLine.AddPolyCollider()) // Add a polygon collider to the line using its lineRenderer points
                    { // AddPolyCollider() returns false if a collision was found inside the object, and the line is destroyed
                        currentLine = null;
                        PlayerVars.instance.ResetTempPenFuel();
                        return;
                    }
                    currentLine.AddPhysics(); // This function also sets the weight of the object based on its area
                    currentLine.SetThickness(penThickness_fin); // Set the thickness of the line
                    currentLine.SetColor(penColor_fin); // Set the color of the line 
                    currentLine.AddMesh(fillMat, fillMatBlock); // Create a mesh from the polygon collider and assign the set material
                    currentLine = null;
                }
                
                else // Otherwise, destroy the line (pen can only create closed loops)
                { 
                    Destroy(currentLine.gameObject);
                    currentLine = null;
                    PlayerVars.instance.ResetTempPenFuel();
                }
            }
        }
        currentLine = null;
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
                currentSoundPause = AudioManager.instance.StartCoroutine(AudioManager.instance.FadeAudioSource(soundPlayer.sources[0], 0.2f, 0f, () => { }));
                soundPaused = true;
            }
        }
        else if (soundPaused)
        {
            if (currentSoundPause != null)
                AudioManager.instance.StopCoroutine(currentSoundPause);
            soundPauseCounter = 0;
            soundPaused = false;
            currentSoundUnpause = AudioManager.instance.StartCoroutine(AudioManager.instance.FadeAudioSource(soundPlayer.sources[0], 0.2f, 1f, () => { }));
        }
    }

    public void SetCursor(ToolType tool)
    {
        Texture2D texture;
        switch (tool)
        {
            case ToolType.Pencil:
                texture = pencilCursor;
                break;
            case ToolType.Pen:
                texture = penCursor;
                break;
            case ToolType.Eraser:
                texture = eraserCursor;
                break;
            default:
                texture = defaultCursor;
                break;
        }
        Cursor.SetCursor(texture, Vector2.zero, CursorMode.ForceSoftware);
    }

    public void SetPencilParams(Line line) {
        line.is_pen = false;
        line.SetThickness(pencilThickness);
        line.collisionsActive = true;
        line.GetComponent<LineRenderer>().startColor = pencilColor_start;
        line.GetComponent<LineRenderer>().endColor = pencilColor_end;
        line.gameObject.layer = 1<<3; // 100 is binary for 8, Lines are on the 8th layer
    }
}



/* Questions:

How are we going to reformat the code?

Do we need to?

*/