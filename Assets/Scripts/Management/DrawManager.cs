using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// Referenced: https://www.youtube.com/watch?v=SmAwege_im8
public class DrawManager : MonoBehaviour
{
    [SerializeField] private Line linePrefab;
    public const float RESOLUTION = 0.1f;
    private Line currentLine;

    // Update is called once per frame
    void Update()
    {
        // Can't draw if you're dead
        if (PlayerController.instance.isDead) return;
        
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Instantiate new line when/where player clicks down
        if (Input.GetMouseButtonDown(0)) currentLine = Instantiate(linePrefab, mousePos, Quaternion.identity);

        // Keep adding line positions
        if (Input.GetMouseButton(0))
        {
            if (currentLine.canDraw || !currentLine.hasDrawn)
            {
                currentLine.SetPosition(mousePos);
            }
            else if (!currentLine.canDraw && currentLine.hasDrawn)
            {
                // We give up on the old line and instantiate a new one, which will wait until it can draw the line
                currentLine = Instantiate(linePrefab, mousePos, Quaternion.identity);
            }
        } 
        // Don't allow single dots to spawn lingering instances
        if (Input.GetMouseButtonUp(0) && currentLine.GetPointsCount() < 2) Destroy(currentLine.gameObject);
    }
}
