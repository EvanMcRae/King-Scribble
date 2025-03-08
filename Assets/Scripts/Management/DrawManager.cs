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
        if (PlayerController.instance.isDead) {
            currentLine = null;
            return;
        }
        
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Instantiate new line when/where player clicks down
        if (Input.GetMouseButtonDown(0)) currentLine = Instantiate(linePrefab, mousePos, Quaternion.identity);

        // Keep adding line positions
        if (Input.GetMouseButton(0))
        {
            if (currentLine != null)
            {
                if (currentLine.canDraw || !currentLine.hasDrawn)
                {
                    // Add the current mouse position to the line's renderer + collider
                    currentLine.SetPosition(mousePos);
                }
                else if (!currentLine.canDraw && currentLine.hasDrawn)
                {
                    // We give up on the old line and instantiate a new one, which will wait until it can draw the line
                    currentLine = Instantiate(linePrefab, mousePos, Quaternion.identity);
                }
            }
            else
            {
                // There was no line in the first place
                currentLine = Instantiate(linePrefab, mousePos, Quaternion.identity);
            }
        }

        // Upon releasing click with a valid line
        if (Input.GetMouseButtonUp(0) && currentLine != null)
        {
            // Don't allow single dots to spawn lingering instances
            if (currentLine.GetPointsCount() < 2)
                Destroy(currentLine.gameObject);

            // Apply physics if there is a closed loop
            else if (currentLine.CheckClosedLoop())
                currentLine.GetComponent<Rigidbody2D>().isKinematic = false;
        }
    }
}
