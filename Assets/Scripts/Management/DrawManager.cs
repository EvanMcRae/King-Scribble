using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// Referenced: https://www.youtube.com/watch?v=SmAwege_im8
public class DrawManager : MonoBehaviour
{
    [SerializeField] private Line linePrefab;
    public const float RESOLUTION = 0.1f;
    public const float DRAW_CD = 0.5f;
    private Line currentLine;
    private float drawCooldown = 0f;
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
        // If the mouse has just been pressed, start drawing
        if (Input.GetMouseButtonDown(0) || (Input.GetMouseButton(0) && currentLine == null))
            BeginDraw(mousePos);
        // If the mouse is continuously held, continue to draw
        if (Input.GetMouseButton(0) && currentLine != null)
            Draw(mousePos);
        // If the mouse has been released, stop drawing
        if (Input.GetMouseButtonUp(0))
            EndDraw();
    }
    private void BeginDraw(Vector2 mouse_pos)
    {
        currentLine = Instantiate(linePrefab, mouse_pos, Quaternion.identity); // Create a new line with the first point at the mouse's current position
        
        // TODO example of setting thickness, to be done later for specific drawing tools
        // currentLine.SetThickness(0.5f);
    }

    private void Draw(Vector2 mouse_pos)
    {
        if (currentLine.canDraw || !currentLine.hasDrawn) { // If the line can draw, create a new point at the mouse's current position
            currentLine.SetPosition(mouse_pos);
            if (currentLine.CheckClosedLoop()) { // If a closed loop has been created with the most recent position added: end the line, enable physics, and start a short cooldown
                EndDraw(); // Enabling physics will take place in this function through a second (admittedly redundant) closed loop check
                drawCooldown = DRAW_CD; // Set a short cooldown (to prevent accidentally drawing a new line immediately after)
            }   
        }
        else if (!currentLine.canDraw && currentLine.hasDrawn) // If the line was stopped by attempting to draw over an unavailable area, continue when available
            BeginDraw(mouse_pos);
    }

    private void EndDraw()
    {
        if (currentLine != null)
        {
            if (currentLine.GetPointsCount() < 2) // Destroy the current line if it is too small
                Destroy(currentLine.gameObject);
            else if (currentLine.CheckClosedLoop()) // Enable physics for the current line if it forms a closed loop (polygon)
            {
                currentLine.AddPhysics();
            }
        }
    }
}

