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
    [SerializeField] private Line linePrefab;
    [SerializeField] private GameObject scribbleMeter;
    [SerializeField] private GameObject penMeter;
    public const float RESOLUTION = 0.1f;
    public const float DRAW_CD = 0.5f;
    private Line currentLine;
    private float drawCooldown = 0f;

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

    public static DrawManager instance;

    private void Awake()
    {
        instance = this;

        Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.ForceSoftware);

        fillMatBlock = new MaterialPropertyBlock();
        fillMatBlock.SetTexture("_MainTex", fillTexture.texture);
        fillMatBlock.SetColor("_Color", fillColor);
        penMeter.GetComponent<Image>().enabled = false;
    }
    // Swap the positions and scales of the scribble and other active tool meters
    void SwapMeters()
    {
        Vector3 tempPosition = scribbleMeter.transform.position;
        Vector3 tempScale = scribbleMeter.GetComponent<RectTransform>().sizeDelta;
        int tempOrder = scribbleMeter.GetComponent<Canvas>().sortingOrder;
        scribbleMeter.transform.position = penMeter.transform.position;
        scribbleMeter.GetComponent<RectTransform>().sizeDelta = penMeter.GetComponent<RectTransform>().sizeDelta;
        scribbleMeter.GetComponent<Canvas>().sortingOrder = penMeter.GetComponent<Canvas>().sortingOrder;
        penMeter.transform.position = tempPosition;
        penMeter.GetComponent<RectTransform>().sizeDelta = tempScale;
        penMeter.GetComponent<Canvas>().sortingOrder = tempOrder;
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

        // Enable the penMeter once the pen is acquired (ugly implementation, too lazy to do it properly right now)
        if (!(penMeter.GetComponent<Image>().enabled) && PlayerVars.instance.inventory.hasTool("Pen"))
        {
            penMeter.GetComponent<Image>().enabled = true;
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
            if (PlayerVars.instance.cur_tool == ToolType.Pencil || PlayerVars.instance.cur_tool == ToolType.Pen)
                BeginDraw(mousePos);
            if (PlayerVars.instance.cur_tool == ToolType.Eraser)
                Erase(mousePos);
        }
        
        // If the mouse is continuously held, continue to draw
        if (Input.GetMouseButton(0) && currentLine != null)
            Draw(mousePos);
        // If the mouse has been released, stop drawing
        if (Input.GetMouseButtonUp(0))
            EndDraw();

        // [1] key pressed - switch to pencil
        if (Input.GetKeyDown("1") && PlayerVars.instance.inventory.hasTool("Pencil") && PlayerVars.instance.cur_tool != ToolType.Pencil)
        {
            if (PlayerVars.instance.cur_tool != ToolType.None)
                SwapMeters();
            if(isDrawing) // checking for if something has interrupted the drawing process while the mouse button is being held down
				EndDraw();
			PlayerVars.instance.cur_tool = ToolType.Pencil;
            Cursor.SetCursor(pencilCursor, Vector2.zero, CursorMode.ForceSoftware);
        }
        // [2] key pressed - switch to pen
        if (Input.GetKeyDown("2") && PlayerVars.instance.inventory.hasTool("Pen") && PlayerVars.instance.cur_tool != ToolType.Pen)
        {
            SwapMeters();
            if(isDrawing) // checking for if something has interrupted the drawing process while the mouse button is being held down
				EndDraw();
			PlayerVars.instance.cur_tool = ToolType.Pen;
            Cursor.SetCursor(penCursor, Vector2.zero, CursorMode.ForceSoftware);
        }
        // [3] key pressed - switch to eraser
        if (Input.GetKeyDown("3") && PlayerVars.instance.inventory.hasTool("Eraser") && PlayerVars.instance.cur_tool != ToolType.Eraser)
        {
            if(isDrawing) // checking for if something has interrupted the drawing process while the mouse button is being held down
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
            currentLine.is_pen = false;
            currentLine.SetThickness(pencilThickness);
            currentLine.collisionsActive = true;
            currentLine.GetComponent<LineRenderer>().startColor = pencilColor_start;
            currentLine.GetComponent<LineRenderer>().endColor = pencilColor_end;
			currentLine.gameObject.layer = 1<<3; // 100 is binary for 8, Lines are on the 8th layer
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
		if(PlayerVars.instance.cur_tool == ToolType.Eraser)
		{
			Erase(mouse_pos);
			return;
		}
		
        if (currentLine.canDraw || !currentLine.hasDrawn) { // If the line can draw, create a new point at the mouse's current position
            currentLine.SetPosition(mouse_pos);

            if (PlayerVars.instance.cur_tool == ToolType.Pen) // If we are drawing with a pen, check for a closed loop
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
                }
            }
        }
		currentLine = null;
    }

    private void Erase(Vector2 mouse_pos) {

        RaycastHit2D[] hit2D = Utils.RaycastAll(Camera.main, mouse_pos + new Vector2(0.5f,-0.5f), LayerMask.GetMask("Lines")); // Raycast is in Utils.cs

        foreach (RaycastHit2D hit in hit2D) {
            // Collider index corresponds to the index in the Line Renderer Array
            CircleCollider2D c = (CircleCollider2D) hit.collider;
            // CircleCollider2D c = Utils.Raycast(Camera.main, mouse_pos, LayerMask.GetMask("Lines"));
            if (c != null) {
                LineRenderer lineRenderer = c.gameObject.GetComponent<LineRenderer>();

                if(lineRenderer != null) {
                    List<CircleCollider2D> collidersList = c.gameObject.GetComponent<Line>().colliders; // List of CircleCollider2D
                    int c_index = collidersList.IndexOf(c); // the collider's index in the list
                    int numPoints = lineRenderer.positionCount; // position count starts at 1 while c_index starts at 0

                    List<Vector3> pointsList = new List<Vector3>(); // Line renderer positions
                    Vector3[] tempArray = new Vector3[numPoints];
                    lineRenderer.GetPositions(tempArray); // Get the positions into the array
                    pointsList.AddRange(tempArray); // Convert tempArray to a list


                    if(c_index == -1) {
                        // ignore the collider because it is no longer a part of the Line object :))
                    }
                    else if( (numPoints <= 2) || (numPoints == 3 && c_index == 1)) { // Destroy the line!
                        //Debug.Log("destroying Line!");
                        PlayerVars.instance.AddDoodleFuel(numPoints);
                        Destroy(c.gameObject);
                        return;
                    }
                    else if(c_index == numPoints - 1 || c_index == 0) { // we are at the edge, delete the first/last point only
                        //Debug.Log("edge detected!");
                        removePoint(c_index, c, pointsList, collidersList);
                    }
                    else if(c_index == 1) {
                       //Debug.Log("2nd to start detected!");
                        removePoint(1, c, pointsList, collidersList);
                        removePoint(0, collidersList[0], pointsList, collidersList);
                    }
                    else if(c_index == numPoints - 2) { // we are at the 2nd to last point, delete the last two point only
                        //Debug.Log("2nd to edge detected!");
                        removePoint(c_index + 1, collidersList[c_index+1], pointsList, collidersList); // Destroy (c+1) first
                        removePoint(c_index, c, pointsList, collidersList);
                    }
                    else { // Create a new Line to fill with the remainder of the points
                        //Debug.Log("Creating new line of size " + (numPoints - c_index+1));
                        Vector3 transformPosition = c.gameObject.GetComponent<Transform>().position;
                        Line newLine = Instantiate(linePrefab, transformPosition, Quaternion.identity);
                        newLine.is_pen = false;
                        newLine.SetThickness(pencilThickness);
                        newLine.collisionsActive = true;
                        newLine.GetComponent<LineRenderer>().startColor = pencilColor_start;
                        newLine.GetComponent<LineRenderer>().endColor = pencilColor_end;
                        newLine.gameObject.layer = 1<<3; // Setting to layer "Lines"
                        
                        // Fill the new line and delete from the current line
                        int currPos = c_index+1; // When we delete a point, we actually dont move in the List
                        for(int i = currPos; i < numPoints; i++) {
                            newLine.SetPosition(pointsList[currPos] + transformPosition, true); // Copy point into a newLine
                            removePoint(currPos, collidersList[currPos], pointsList, collidersList, false);
                        }
                                      
                        //Debug.Log("Deleting current point");
                        removePoint(c_index, c, pointsList, collidersList); // Delete the current collider

                        // sometimes there are stray colliders with no lines, could be that lines of size 1 cannot render the points
                        // There is a bug where empty line clones are being left behind, only occurs on newly generated lines i think
                    }

                    // Update the current Line Renderer
                    lineRenderer.positionCount = pointsList.Count;
                    lineRenderer.SetPositions(pointsList.ToArray());

                    // Extra check for good measure
                    if (pointsList.Count <= 1)
                    {
                        PlayerVars.instance.AddDoodleFuel(pointsList.Count);
                        Destroy(c.gameObject);
                    }
                }
            }
       }
    }

    private void removePoint (int index, CircleCollider2D c, List<Vector3> pl, List<CircleCollider2D> cl, bool addFuel = true) {
        pl.RemoveAt(index); // Remove point from the list
        //Debug.Log("destroying: " + index);
        cl.RemoveAt(index); // Remove collider from the list
        Destroy(c); // Destroy collider
        if (addFuel) PlayerVars.instance.AddDoodleFuel(1); // Add fuel
        return;
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
}



/* Questions:

How are we going to reformat the code?

*/