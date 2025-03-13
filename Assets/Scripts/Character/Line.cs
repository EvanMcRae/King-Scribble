using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Referenced: https://www.youtube.com/watch?v=SmAwege_im8
public class Line : MonoBehaviour
{
    [SerializeField] private GameObject linePoint;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Rigidbody2D rigidBody;
    public List<CircleCollider2D> colliders = new(); // TODO use this for eraser checking?
    public bool canDraw = true, hasDrawn = false;
    public const float LOOP_ALLOWANCE = 0.2f; // Maximum distance between the first and last point of a line to be considered a closed loop
    public const int MIN_POINTS = 8; // Minimum points on a line for it to be considered a closed loop
    public float thickness = 0.1f; // How wide the line will be drawn
    public bool collisionsActive = true; // If collisions are active while drawing (for pen - initially false, set to true on finish)
    public bool is_pen = false;

    // Start is called before the first frame update
    void Start()
    {
        if (rigidBody != null)
            rigidBody.isKinematic = true;

        lineRenderer.widthMultiplier = thickness;
    }

    public void SetPosition(Vector2 position)
    {
        if (!CanAppend(position)) return;

        position = transform.InverseTransformPoint(position);

        // If this point is too far away, march along it and add extra points
        if (lineRenderer.positionCount > 0 && Vector2.Distance(GetLastPoint(), position) > DrawManager.RESOLUTION)
        {
            Vector2 marchPos = GetLastPoint();
            do
            {
                marchPos = Vector2.MoveTowards(marchPos, position, DrawManager.RESOLUTION);
                AppendPos(marchPos);
            } while (Vector2.Distance(marchPos, position) > DrawManager.RESOLUTION);
        }

        AppendPos(position);

        hasDrawn = true;
    }

    private void AppendPos(Vector2 position)
    {
        // Add circle collider component for this point
        if (!is_pen) {
            CircleCollider2D circleCollider = gameObject.AddComponent<CircleCollider2D>();
            circleCollider.offset = position;
            circleCollider.radius = thickness / 2;
            if (!collisionsActive) circleCollider.enabled = false;
            colliders.Add(circleCollider);
        }
        // Add line renderer position for this point
        lineRenderer.positionCount++;
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, position);

        // Deduct doodle fuel if there's more than one point on this line
        if (lineRenderer.positionCount > 1) PlayerController.instance.DrawDoodleFuel(1);
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

        // Then check for minimum distance between points with local space-transformed position
        return Vector2.Distance(GetLastPoint(), transform.InverseTransformPoint(position)) > DrawManager.RESOLUTION;
    }
    public int GetPointsCount()
    {
        return lineRenderer.positionCount;
    }
    public Vector2 GetFirstPoint()
    {
        return lineRenderer.GetPosition(0);
    }
    public Vector2 GetLastPoint()
    {
        return lineRenderer.GetPosition(GetPointsCount() - 1);
    }
    public bool CheckClosedLoop()
    {
        return GetPointsCount() >= MIN_POINTS && Vector2.Distance(GetFirstPoint(), GetLastPoint()) <= LOOP_ALLOWANCE;
    }
    public void AddPhysics()
    {
        /*
        // Properly connect close loops if within allowance
        Vector2 marchPos = GetLastPoint();
        while (Vector2.Distance(marchPos, GetFirstPoint()) > DrawManager.RESOLUTION)
        {
            marchPos = Vector2.MoveTowards(marchPos, GetFirstPoint(), DrawManager.RESOLUTION);
            AppendPos(marchPos);
        }
        */
        lineRenderer.SetPosition(GetPointsCount()-1, GetFirstPoint());
        // Apply physics behavior
        GetComponent<Rigidbody2D>().isKinematic = false;

        // Set weight based on area
        Vector3[] points = new Vector3[GetPointsCount()]; 
        lineRenderer.GetPositions(points); // Get an array containing all points in the line
        // Note: This is ugly. I know this is ugly. It works. (from https://stackoverflow.com/questions/2034540/calculating-area-of-irregular-polygon-in-c-sharp)
        var area = Mathf.Abs(points.Take(GetPointsCount() - 1).Select((p, i) => (points[i + 1].x - p.x) * (points[i + 1].y + p.y)).Sum() / 2);
        rigidBody.mass = area;
    }
    public bool CheckCollision()
    // Check if the most recently drawn point is within some small distance from any other point (aka, if the user has created a loop - closed or otherwise)
    {
        for (int i = 0; i < GetPointsCount() - 2; i++) // 2 instead of 1 to prevent detecting a collision with the previously drawn point (which would otherwise always happen)
        {
            if (Vector2.Distance((Vector2)lineRenderer.GetPosition(i), GetLastPoint()) < 0.08f)
                return true;
        }

        return false;
    }
    public void EnableColliders()
    {
        foreach (CircleCollider2D c in colliders)
            c.enabled = true;
    }
    // TODO Could be used in the future for other tools
    public void SetThickness(float newThickness)
    {
        thickness = newThickness;
        lineRenderer.widthMultiplier = thickness;

        // If this is run after the line has been drawn, retroactively update colliders too
        if (lineRenderer.positionCount > 0)
        {
            foreach (CircleCollider2D circleCollider in colliders)
            {
                circleCollider.radius = thickness / 2;
            }
        }
    }
}
