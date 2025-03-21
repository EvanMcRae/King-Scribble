using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


// Referenced: https://www.youtube.com/watch?v=SmAwege_im8
public enum ToolType
{
    Pencil,
    Pen,
    Eraser
}
public class DrawManager : MonoBehaviour
{
    [SerializeField] private Line linePrefab;
    public const float RESOLUTION = 0.1f;
    public const float DRAW_CD = 0.5f;
    private Line currentLine;
    private float drawCooldown = 0f;
    public GameObject player; // For accessing the player's available tools (and other player vars)
    public bool isDrawing = false; // True when the mouse is being held down with an drawing tool
    public bool isErasing = false; // True when the mouse is being held down with an erasing tool
    public ToolType cur_tool = ToolType.Pencil;
    public Color pencilColor; // Color of pencil lines
    public Color penColor_start; // Color of pen lines while being drawn
    public Color penColor_fin; // Color of pen lines once finished
    public float pencilThickness; // Thickness of pencil lines
    public float penThickness_start; // Thickness of pen lines while being drawn
    public float penThickness_fin; // Thickness of pen lines once finished
    public Texture2D pencilCursor; // The texture file for the cursor used for the pencil
    public Texture2D penCursor; // The texture file for the cursor used for the pen
    public Texture2D eraserCursor; // The texture file for the cursor used for the eraser

    public Material fillMat; // The material to fill pen objects with (temporary)
    public Sprite fillTexture; // The texture to fill pen objects with (temporary)
    public Color fillColor; // The color to fill pen objects with (temporary)
    private MaterialPropertyBlock fillMatBlock; // Material property overrides for pen fill (temporary)

    private void Awake()
    {
        fillMatBlock = new MaterialPropertyBlock();
        fillMatBlock.SetTexture("_MainTex", fillTexture.texture);
        fillMatBlock.SetColor("_Color", fillColor);
    }

    // Update is called once per frame
    void Update()
    {
        // Can't draw if you're dead
        if (PlayerController.instance.isDead) {
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
            if (cur_tool == ToolType.Pencil || cur_tool == ToolType.Pen)
                BeginDraw(mousePos);
            if (cur_tool == ToolType.Eraser)
                BeginErase(mousePos);
        }
        
        // If the mouse is continuously held, continue to draw
        if (Input.GetMouseButton(0) && currentLine != null)
            Draw(mousePos);
        // If the mouse has been released, stop drawing
        if (Input.GetMouseButtonUp(0))
            EndDraw();

        // [1] key pressed - switch to pencil
        if (Input.GetKeyDown("1") && player.GetComponent<PlayerVars>().inventory.hasTool("Pencil"))
        {
            if(isDrawing) // checking for if something has interrupted the drawing process while the mouse button is being held down
				EndDraw();
			cur_tool = ToolType.Pencil;
            Cursor.SetCursor(pencilCursor, Vector2.zero, CursorMode.ForceSoftware);
        }
        // [2] key pressed - switch to pen
        if (Input.GetKeyDown("2") && player.GetComponent<PlayerVars>().inventory.hasTool("Pen"))
        {
            if(isDrawing) // checking for if something has interrupted the drawing process while the mouse button is being held down
				EndDraw();
			cur_tool = ToolType.Pen;
            Cursor.SetCursor(penCursor, Vector2.zero, CursorMode.ForceSoftware);
        }
        // [3] key pressed - switch to eraser
        if (Input.GetKeyDown("3") && player.GetComponent<PlayerVars>().inventory.hasTool("Eraser"))
        {
            if(isDrawing) // checking for if something has interrupted the drawing process while the mouse button is being held down
				EndDraw();
			cur_tool = ToolType.Eraser;
            Cursor.SetCursor(eraserCursor, Vector2.zero, CursorMode.ForceSoftware);
        }
        
    }
    private void BeginDraw(Vector2 mouse_pos)
    {	
        currentLine = Instantiate(linePrefab, mouse_pos, Quaternion.identity); // Create a new line with the first point at the mouse's current position
		isDrawing = true; // the user is drawing
        if (cur_tool == ToolType.Pencil) {
            currentLine.is_pen = false;
            currentLine.SetThickness(pencilThickness);
            currentLine.collisionsActive = true;
            currentLine.GetComponent<LineRenderer>().startColor = pencilColor;
            currentLine.GetComponent<LineRenderer>().endColor = pencilColor;
        }

        else if (cur_tool == ToolType.Pen) {
            currentLine.is_pen = true;
            currentLine.SetThickness(penThickness_start);
            currentLine.collisionsActive = false;
            currentLine.GetComponent<LineRenderer>().startColor = penColor_start;
            currentLine.GetComponent<LineRenderer>().endColor = penColor_start;
        }

    }

    private void Draw(Vector2 mouse_pos)
    {
		if(cur_tool == ToolType.Eraser)
		{
			BeginErase(mouse_pos);
			return;
		}
		
        if (currentLine.canDraw || !currentLine.hasDrawn) { // If the line can draw, create a new point at the mouse's current position
            currentLine.SetPosition(mouse_pos);

            if (cur_tool == ToolType.Pen) // If we are drawing with a pen, check for a closed loop
            {
                if (currentLine.CheckClosedLoop() || currentLine.CheckCollision()) // If a closed loop or collision is created: end the line, enable physics, and start a short cooldown
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

            if (cur_tool == ToolType.Pen) // If we are drawing with a pen, check for a closed loop
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
                }
            }
        }
		currentLine = null;
    }

    private void BeginErase(Vector2 mouse_pos) {
		// need to shift to the left 8 times to get the layer mask of layer 8
		// Ground is layer 3
		//Debug.Log("Beginning Eraser");

		/* Where I left off:
			objects on the layer are not being detected~ unsure why but i'll experiment with OverlapPoint instead of Raycast next time :))
			maybe the mouse_pos isn't the "point" parameter we need

			I assure that the mouse_pos = the collision pos
		*/
    	GameObject g = Utils.Raycast(Camera.main, mouse_pos, 1<<3); // Raycast is in Utils.cs
		Debug.Log(mouse_pos);
		if (g != null)
			Debug.Log("Destroying!! ", g);

		// Also need to call this while the mouse is being held down!

		
    }
}
