using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Must;


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
    // TEMPORARY - REPLACE WITH AN ARRAY OR SOMETHING SOMEWHERE ELSE
    public bool hasPencil = true;
    public bool hasPen = false;
    public bool hasEraser = false;
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
    
    Vector2[] ConvertArray(Vector3[] v3){
        Vector2 [] v2 = new Vector2[v3.Length];
        for(int i = 0; i <  v3.Length; i++){
            Vector3 tempV3 = v3[i];
            v2[i] = new Vector2(tempV3.x, tempV3.y);
        }
        return v2;
    }

    public Texture2D eraserCursor; // The texture file for the cursor used for the eraser

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
            
        // [1] key pressed - switch to pencil
        if (Input.GetKeyDown("1"))
        {
            if(isDrawing) // checking for if something has interrupted the drawing process while the mouse button is being held down
				EndDraw();
			cur_tool = ToolType.Pencil;
            Cursor.SetCursor(pencilCursor, Vector2.zero, CursorMode.ForceSoftware);
        }
        // [2] key pressed - switch to pen
        if (Input.GetKeyDown("2"))
        {
            if(isDrawing) // checking for if something has interrupted the drawing process while the mouse button is being held down
				EndDraw();
			cur_tool = ToolType.Pen;
            Cursor.SetCursor(penCursor, Vector2.zero, CursorMode.ForceSoftware);
        }
        // [3] key pressed - switch to eraser
        if (Input.GetKeyDown("3"))
        {
            if(isDrawing) // checking for if something has interrupted the drawing process while the mouse button is being held down
				EndDraw();
			cur_tool = ToolType.Eraser;
            Cursor.SetCursor(eraserCursor, Vector2.zero, CursorMode.ForceSoftware);
        }
        
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // If the mouse has just been pressed, start drawing
        if (Input.GetMouseButtonDown(0) || (Input.GetMouseButton(0) && currentLine == null))
        {
			if(cur_tool == ToolType.Pencil || cur_tool == ToolType.Pen)
				BeginDraw(mousePos);
			if(cur_tool == ToolType.Eraser)
				Erase(mousePos);
        }
        // If the mouse is continuously held, continue to draw
        if (Input.GetMouseButton(0) && currentLine != null)
		{
			Draw(mousePos);
		}
		// If the mouse has been released, stop drawing
        if (Input.GetMouseButtonUp(0))
		{
			if(isDrawing) // checking for if something has interrupted the drawing process while the mouse button is being held down
				EndDraw();
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
			currentLine.gameObject.layer = 1<<3; // 100 is binary for 8, Lines are on the 8th layer
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
			Erase(mouse_pos);
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
                    currentLine.SetThickness(penThickness_fin);
                    currentLine.GetComponent<LineRenderer>().startColor = penColor_fin;
                    currentLine.GetComponent<LineRenderer>().endColor = penColor_fin;
                    // currentLine.EnableColliders();
                    var polyCollider = currentLine.gameObject.AddComponent<PolygonCollider2D>();
                    Vector3[] points = new Vector3[currentLine.GetPointsCount()];
                    Vector2[] pointsv2 = new Vector2[currentLine.GetPointsCount()];
                    currentLine.GetComponent<LineRenderer>().GetPositions(points);
                    pointsv2 = ConvertArray(points);
                    polyCollider.SetPath(0, pointsv2);
                    currentLine = null;

                }
                
                else // Otherwise, destroy the line (pen can only create closed loops)
                { 
                    Destroy(currentLine.gameObject);
                }
            }
        }
		currentLine = null;
    }

    private void Erase(Vector2 mouse_pos) {

    	CircleCollider2D c = Utils.Raycast(Camera.main, mouse_pos, LayerMask.GetMask("Lines")); // Raycast is in Utils.cs
       
        // Collider index corresponds to the index in the Line Renderer Array
		if (c != null) {
            LineRenderer lineRenderer = c.gameObject.GetComponent<LineRenderer>();

            if(lineRenderer != null) {
                List<CircleCollider2D> collidersList = c.gameObject.GetComponent<Line>().colliders; // List of CircleCollider2D
                int c_index = collidersList.IndexOf(c);

                List<Vector3> points = new List<Vector3>(); // Line renderer positions
                Vector3[] pointsArray = new Vector3[lineRenderer.positionCount];
                lineRenderer.GetPositions(pointsArray); // Get the positions into the array
                points.AddRange(pointsArray); // Convert pointsArray to a list

                int size = 0;
                // size of our points vector ( i forget the method xD)
                foreach (Vector3 point in points)
                {
                    size++;
                }

                Debug.Log("size is " + size + "\nc_index is " + c_index);
                Debug.Log("position count: " + lineRenderer.positionCount); // position count starts at 1 while c_index starts at 0

                if((size == 2) || (size == 3 && c_index == 1)) { // Destroy the line!
                    Debug.Log("Line will be too small, destroying Line!");
                    Destroy(c.gameObject);
                    c = null;
                    return;
                }
                else if(c_index == lineRenderer.positionCount - 1 || c_index == 0) { // we are at the edge, delete the first/last point only
                    Debug.Log("edge detected!");

                    points.RemoveAt(c_index); // Remove point
                    Debug.Log("destroying: " + c_index);
                    Destroy(c); // Destroy collider
                    collidersList.RemoveAt(c_index);
                }
                else if(c_index == 1) {
                    Debug.Log("2nd to start detected!");
                    
                    points.RemoveAt(1);
                    Debug.Log("destroying: " + 1);
                    Destroy(c);
                    collidersList.RemoveAt(1);

                    points.RemoveAt(0);
                    Debug.Log("destroying: 0");
                    Destroy(collidersList[0]);
                    collidersList.RemoveAt(0);
                }
                else if(c_index == lineRenderer.positionCount - 2) { // we are at the 2nd to last point, delete the last two point only
                    Debug.Log("2nd to edge detected!");

                    points.RemoveAt(c_index+1);
                    Debug.Log("destroying: " + c_index+1);
                    Destroy(collidersList[c_index+1]); // Destroy (c+1)!!!
                    collidersList.RemoveAt(c_index+1);

                    points.RemoveAt(c_index);
                    Debug.Log("destroying: " + c_index);
                    Destroy(c);
                    collidersList.RemoveAt(c_index);
                }
                else {
                    // Optimizing the split:
                    if(lineRenderer.positionCount/2.0 <= c_index) { // delete the left side of the Vector3

                    }
                    else { // delete the right part

                    }
                    Debug.Log("collider position is " + c.bounds.center);
                    // Create a new Line to fill with the remainder of the points
                    Debug.Log("Creating new line");
                    Line newLine = Instantiate(linePrefab, c.bounds.center, Quaternion.identity);
                    newLine.is_pen = false;
                    newLine.SetThickness(pencilThickness);
                    newLine.collisionsActive = true;
                    newLine.GetComponent<LineRenderer>().startColor = pencilColor;
                    newLine.GetComponent<LineRenderer>().endColor = pencilColor;
                    newLine.gameObject.layer = 1<<3; // Setting to layer "Lines"

                    // Fill the new line and delete from the current line
                    int positionCount = lineRenderer.positionCount;
                    int currPos = c_index+1; // When we delete a point, we actually dont move in the List
                    for(int i = c_index+1; i < positionCount; i++) {
                        Debug.Log("points[currPos] " + (points[currPos] + collidersList[0].bounds.center));
                        newLine.SetPosition(points[currPos] + collidersList[0].bounds.center); // Copy point into a newLine

                        points.RemoveAt(currPos); // Delete the point
                        Debug.Log("destroying: " + currPos);
                        Destroy(collidersList[currPos]); // Delete the collider
                        collidersList.RemoveAt(currPos);
                    }
                    

                    // checkpoint: change CircleCast to CircleCastAll so we can have a bigger eraser size?
                    // line colliders and points have an offset when instantiated
                    // rn it is points[currPos] + c.bounds.center 
                    // points[currPos] + collidersList[0].bounds.center works better!
                }
                // Update the current Line Renderer
                lineRenderer.positionCount = points.Count;
                lineRenderer.SetPositions(points.ToArray());
            }
            c = null;
		}
    }
}


/* Questions:

How are we going to reformat the code?
Eraser currently depletes health...

*/