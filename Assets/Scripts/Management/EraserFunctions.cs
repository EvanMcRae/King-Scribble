using System.Collections.Generic;
using UnityEngine;

public class EraserFunctions : MonoBehaviour
{
    public static void Erase(Vector2 pos, float radius, bool addFuel, GameObject parent = null) {

        RaycastHit2D[] hit2D = Utils.RaycastAll(Camera.main, pos, LayerMask.GetMask("Lines"), radius); // Raycast is in Utils.cs

        for (int h = 0; h < hit2D.Length; h++) {
            RaycastHit2D hit = hit2D[h];
            // Collider index corresponds to the index in the Line Renderer Array
            CircleCollider2D c = (CircleCollider2D) hit.collider;
            // CircleCollider2D c = Utils.Raycast(Camera.main, mouse_pos, LayerMask.GetMask("Lines"));
            if (c != null) {
                LineRenderer lineRenderer = c.gameObject.GetComponent<LineRenderer>();

                if(lineRenderer != null) {
                    DrawManager.instance.CheckRefreshLine(c.GetComponent<Line>());
                    List<CircleCollider2D> collidersList = c.gameObject.GetComponent<Line>().colliders; // List of CircleCollider2D
                    int c_index = collidersList.IndexOf(c); // the collider's index in the list
                    int numPoints = lineRenderer.positionCount; // position count starts at 1 while c_index starts at 0

                    List<Vector3> pointsList = new List<Vector3>(); // Line renderer positions
                    Vector3[] tempArray = new Vector3[numPoints];
                    lineRenderer.GetPositions(tempArray); // Get the positions into the array
                    pointsList.AddRange(tempArray); // Convert tempArray to a list

                    if(c_index == -1) {
                        //Debug.Log(lineRenderer.gameObject.GetInstanceID() + ": " + "invalid case");
                        // ignore the collider because it is no longer a part of the Line object :))
                    }
                    else if (lineRenderer.GetComponent<Line>().deleted)
                    {
                        continue; // skip already deleted lines
                    }
                    else if( (numPoints <= 2) || (numPoints == 3 && c_index == 1)) { // Destroy the line!
                        //Debug.Log(lineRenderer.gameObject.GetInstanceID() + ": " + "destroying line with " + numPoints + " points");
                        PlayerVars.instance.AddDoodleFuel(numPoints);
                        PlayerVars.instance.SpendEraserFuel(numPoints);
                        Destroy(c.gameObject);
                        lineRenderer.GetComponent<Line>().deleted = true;
                        continue;
                    }
                    else if(c_index == numPoints - 1 || c_index == 0) { // we are at the edge, delete the first/last point only
                        //Debug.Log(lineRenderer.gameObject.GetInstanceID() + ": " + "edge detected! removing 1 point");
                        removePoint(c_index, c, pointsList, collidersList, addFuel);
                    }
                    else if(c_index == 1) {
                        //Debug.Log(lineRenderer.gameObject.GetInstanceID() + ": " + "2nd to start detected!");
                        removePoint(1, c, pointsList, collidersList, addFuel);
                        removePoint(0, collidersList[0], pointsList, collidersList, addFuel);
                    }
                    else if(c_index == numPoints - 2) { // we are at the 2nd to last point, delete the last two point only
                        //Debug.Log(lineRenderer.gameObject.GetInstanceID() + ": " + "2nd to edge detected!");
                        removePoint(c_index + 1, collidersList[c_index+1], pointsList, collidersList, addFuel); // Destroy (c+1) first
                        removePoint(c_index, c, pointsList, collidersList, addFuel);
                    }
                    else { // Create a new Line to fill with the remainder of the points
                        //Debug.Log("Creating new line of size " + (numPoints - c_index - 1));
                        Line newLine;
                        Vector3 transformPosition = c.gameObject.GetComponent<Transform>().position;
                        if(parent != null) {
                            newLine = Instantiate(DrawManager.instance.linePrefab, transformPosition, Quaternion.identity, parent.transform);
                        }
                        else {
                            newLine = Instantiate(DrawManager.instance.linePrefab, transformPosition, Quaternion.identity);
                        }
                        
                        DrawManager.instance.SetPencilParams(newLine);
                        DrawManager.instance.SwapColors(newLine);

                        // Fill the new line and delete from the current line
                        int ct = 0;
                        for(int i = numPoints - 1; i >= c_index + 1; i--) {
                            ct++;
                            newLine.SetPosition(pointsList[i] + transformPosition, true, false); // Copy point into a newLine
                            removePoint(i, collidersList[i], pointsList, collidersList, false);
                            //Debug.Log(newLine.GetInstanceID() + " size " + ct + " == " + newLine.GetComponent<LineRenderer>().positionCount); ;
                        }
                        //Debug.Log(newLine.GetInstanceID() + " new line of size " + ct); ;

                        removePoint(c_index, c, pointsList, collidersList, addFuel); // Delete the current collider

                        hit2D = Utils.RaycastAll(Camera.main, pos, LayerMask.GetMask("Lines"), radius);
                        h = 0;

                        // sometimes there are stray colliders with no lines, could be that lines of size 1 cannot render the points
                        // There is a bug where empty line clones are being left behind, only occurs on newly generated lines i think
                    }

                    // Update the current Line Renderer
                    lineRenderer.positionCount = pointsList.Count;
                    lineRenderer.SetPositions(pointsList.ToArray());
                    lineRenderer.GetComponent<Line>().RefreshEdge();

                    // Extra check for good measure
                    if (pointsList.Count <= 1)
                    {
                        //Debug.Log(lineRenderer.gameObject.GetInstanceID() + ": " + "Destroying line with " + pointsList.Count + " points");
                        if (addFuel) {
                            PlayerVars.instance.AddDoodleFuel(pointsList.Count);
                            PlayerVars.instance.SpendEraserFuel(pointsList.Count);
                        }
                        lineRenderer.GetComponent<Line>().deleted = true;
                        Destroy(c.gameObject);
                    }
                }
            }
       }
    }

    private static void removePoint (int index, CircleCollider2D c, List<Vector3> pl, List<CircleCollider2D> cl, bool addFuel) {
        pl.RemoveAt(index); // Remove point from the list
        cl.RemoveAt(index); // Remove collider from the list
        c.GetComponent<Line>().points.RemoveAt(index); // Remove point from the other 2D list
        Destroy(c); // Destroy collider

        if (addFuel)
        {
            PlayerVars.instance.AddDoodleFuel(1); // Add fuel
            PlayerVars.instance.SpendEraserFuel(1); // Spend eraser
        }
        return;
    }
}
