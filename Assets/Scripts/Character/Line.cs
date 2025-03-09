using System.Collections;
using System.Collections.Generic;
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
    public const float OVERSHOOT = 0.025f; // How much a line march will attempt to overshoot over default resolution
    public float thickness = 0.1f; // How wide the line will be drawn
    public bool collisionsActive = true; // If collisions are active while drawing (for pen - initially false, set to true on finish)

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
        if (lineRenderer.positionCount > 0 && Vector2.Distance(GetLastPoint(), position) > DrawManager.RESOLUTION + OVERSHOOT)
        {
            Vector2 marchPos = GetLastPoint();
            do
            {
                marchPos = Vector2.MoveTowards(marchPos, position, DrawManager.RESOLUTION + OVERSHOOT);
                AppendPos(marchPos);
            } while (Vector2.Distance(marchPos, position) > DrawManager.RESOLUTION + OVERSHOOT);
        }

        AppendPos(position);

        hasDrawn = true;
    }

    private void AppendPos(Vector2 position)
    {
        // Add circle collider component for this point
        CircleCollider2D circleCollider = gameObject.AddComponent<CircleCollider2D>();
        circleCollider.offset = position;
        circleCollider.radius = thickness / 2;
        if (!collisionsActive) circleCollider.enabled = false;
        colliders.Add(circleCollider);

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
        // Properly connect close loops if within allowance
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, lineRenderer.GetPosition(0));

        // Apply physics behavior
        GetComponent<Rigidbody2D>().isKinematic = false;
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
                circleCollider.radius = thickness;
            }
        }
    }
}
