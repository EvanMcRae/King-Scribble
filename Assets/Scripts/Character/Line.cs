using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Referenced: https://www.youtube.com/watch?v=SmAwege_im8
public class Line : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private EdgeCollider2D cldr;
    private readonly List<Vector2> points = new List<Vector2>();

    // Start is called before the first frame update
    void Start()
    {
        cldr.transform.position = Vector3.zero; 
    }

    public void SetPosition(Vector2 position)
    {
        if (!CanAppend(position)) return;

        points.Add(position);
        lineRenderer.positionCount++;
        lineRenderer.SetPosition(lineRenderer.positionCount-1, position);

        cldr.points = points.ToArray();
    }

    private bool CanAppend(Vector2 position)
    {
        // Unable to draw lines inside yourself
        if (Vector2.Distance(position, PlayerController.instance.transform.position) < 1.1f) return false;

        // If not inside yourself, first point is always ok
        if (lineRenderer.positionCount == 0) return true;

        // Then check for resolution
        return Vector2.Distance(lineRenderer.GetPosition(lineRenderer.positionCount-1), position) > DrawManager.RESOLUTION;
    }
    public int GetPointsCount()
    {
        return points.Count;
    }
}
