using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Referenced: https://www.youtube.com/watch?v=SmAwege_im8
public class Line : MonoBehaviour
{
    [SerializeField] private GameObject linePoint;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private EdgeCollider2D cldr;
    private readonly List<Transform> pointTransforms = new List<Transform>();
    private readonly List<Vector2> points = new List<Vector2>();
    public bool canDraw = true, hasDrawn = false;
    private Transform previousPos;

    // Start is called before the first frame update
    void Start()
    {
        cldr.transform.position = Vector3.zero; 
    }

    void Update()
    {
        // Checks for if line actually moved
        // TODO check for if line is movable instead (i.e. closed loop)
        if (transform.Equals(previousPos))
        {
            for (int i = 0; i < points.Count; i++)
            {
                if (pointTransforms.Count >= i)
                {
                    lineRenderer.SetPosition(i, pointTransforms[i].position);
                }
            }
        }
        
        previousPos = transform;
    }

    public void SetPosition(Vector2 position)
    {
        if (!CanAppend(position)) return;

        if (lineRenderer.positionCount > 0) PlayerController.instance.DrawDoodleFuel(1);

        GameObject newPoint = Instantiate(linePoint, gameObject.transform, false);
        newPoint.transform.position = position;
        pointTransforms.Add(newPoint.transform);
        points.Add(position);
        lineRenderer.positionCount++;
        lineRenderer.SetPosition(lineRenderer.positionCount-1, position);
        cldr.points = points.ToArray();

        hasDrawn = true;
    }

    private bool CanAppend(Vector2 position)
    {
        // Unable to draw lines inside yourself
        if (PlayerController.instance.OverlapsPosition(position))
        {
            canDraw = false;
            return false;
        }

        canDraw = true;

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
