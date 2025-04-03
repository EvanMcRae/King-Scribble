using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

// Referenced: https://www.youtube.com/watch?v=SmAwege_im8
public enum ToolType
{
    None,
    Pencil,
    Pen,
    Eraser
}
public class DrawManager : MonoBehaviour
{
    [SerializeField] public Line linePrefab;
    [SerializeField] public float eraserRadius = 0.5f; // radius of the raycast of what will be erased
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

    [SerializeField] private List<GameObject> submeters;

    public static DrawManager instance;

    private void Awake()
    {
        instance = this;

        SetCursor(PlayerVars.instance.cur_tool);

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
        // Can't draw if you're dead
        if (PlayerVars.instance.isDead) {
            currentLine = null;
            return;
        }
        // If the drawing cooldown is active, decrement it and don't do anything
        if (drawCooldown > 0) { 
            drawCooldown -= Time.deltaTime;
            return;
        }

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (PlayerController.instance.OverlapsPosition(mousePos))
        {
            EndDraw();
            currentLine = null;
        }

        // If the mouse has just been pressed, start drawing
        if (Input.GetMouseButtonDown(0) || (Input.GetMouseButton(0) && currentLine == null))
        {
            switch (PlayerVars.instance.cur_tool)
            {
                case ToolType.Pencil:
                    if (PlayerVars.instance.doodleFuelLeft() > 0) BeginDraw(mousePos);
                    break;
                case ToolType.Pen:
                    if (PlayerVars.instance.penFuelLeft() > 0) BeginDraw(mousePos);
                    break;
                case ToolType.Eraser:
                    if (PlayerVars.instance.eraserFuelLeft() > 0) EraserFunctions.Erase(mousePos + new Vector2(0.5f,-0.5f), eraserRadius, true);
                    break;
            }
        }
        
        // If the mouse is continuously held, continue to draw
        if (Input.GetMouseButton(0) && currentLine != null)
            Draw(mousePos);
        // If the mouse has been released, stop drawing
        if (Input.GetMouseButtonUp(0))
        {
            EndDraw();
            if (PlayerVars.instance.cur_tool == ToolType.Eraser)
            {
                PlayerVars.instance.releaseEraser?.Invoke();
            }
        }
            

        // [1] key pressed - switch to pencil
        if (Input.GetKeyDown("1") && PlayerVars.instance.inventory.hasTool(ToolType.Pencil) && PlayerVars.instance.cur_tool != ToolType.Pencil)
        {
            LoadSubmeter(ToolType.Pencil);
            if (isDrawing) // checking for if something has interrupted the drawing process while the mouse button is being held down
				EndDraw();
			PlayerVars.instance.cur_tool = ToolType.Pencil;
            Cursor.SetCursor(pencilCursor, Vector2.zero, CursorMode.ForceSoftware);
        }
        // [2] key pressed - switch to pen
        if (Input.GetKeyDown("2") && PlayerVars.instance.inventory.hasTool(ToolType.Pen) && PlayerVars.instance.cur_tool != ToolType.Pen)
        {
            LoadSubmeter(ToolType.Pen);
            if (isDrawing) // checking for if something has interrupted the drawing process while the mouse button is being held down
				EndDraw();
			PlayerVars.instance.cur_tool = ToolType.Pen;
            Cursor.SetCursor(penCursor, Vector2.zero, CursorMode.ForceSoftware);
        }
        // [3] key pressed - switch to eraser
        if (Input.GetKeyDown("3") && PlayerVars.instance.inventory.hasTool(ToolType.Eraser) && PlayerVars.instance.cur_tool != ToolType.Eraser)
        { 
            LoadSubmeter(ToolType.Eraser);
            if (isDrawing) // checking for if something has interrupted the drawing process while the mouse button is being held down
				EndDraw();
			PlayerVars.instance.cur_tool = ToolType.Eraser;
            Cursor.SetCursor(eraserCursor, Vector2.zero, CursorMode.ForceSoftware);
        }
    }
    private void BeginDraw(Vector2 mouse_pos)
    {	
        currentLine = Instantiate(linePrefab, mouse_pos, Quaternion.identity); // Create a new line with the first point at the mouse's current position
		isDrawing = true; // the user is drawing
        if (PlayerVars.instance.cur_tool == ToolType.Pencil) {
            SetPencilParams(currentLine);
        }

        else if (PlayerVars.instance.cur_tool == ToolType.Pen) {
            currentLine.is_pen = true;
            currentLine.SetThickness(penThickness_start);
            currentLine.collisionsActive = false;
            currentLine.GetComponent<LineRenderer>().startColor = penColor_start;
            currentLine.GetComponent<LineRenderer>().endColor = penColor_start;
        }

    }

    private void Draw(Vector2 mouse_pos)
    {
		if (PlayerVars.instance.cur_tool == ToolType.Eraser)
		{
            if (PlayerVars.instance.eraserFuelLeft() > 0)
                EraserFunctions.Erase(mouse_pos + new Vector2(0.5f,-0.5f), eraserRadius, true);
            else EndDraw();
			return;
		}
		
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
    }

    private void EndDraw()
    {
        isDrawing = false; // the user has stopped drawing

        if (currentLine != null)
        {
            if (currentLine.GetPointsCount() < 2) // Destroy the current line if it is too small
                Destroy(currentLine.gameObject);

            if (PlayerVars.instance.cur_tool == ToolType.Pen) // If we are drawing with a pen, check for a closed loop
            {
                if (currentLine.CheckClosedLoop()) // If the line is a closed loop: enable physics, set width and color to final parameters, and set weight based on area of the drawn polygon
                {
                    currentLine.AddPhysics(); // This function also sets the weight of the object based on its area
                    currentLine.SetThickness(penThickness_fin); // Set the thickness of the line
                    currentLine.SetColor(penColor_fin); // Set the color of the line 
                    currentLine.AddPolyCollider(); // Add a polygon collider to the line using its lineRenderer points
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

*/