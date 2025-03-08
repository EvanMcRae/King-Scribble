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
    public const float LOOP_ALLOWANCE = 0.1f; // Maximum distance between the first and last point of a line to be considered a closed loop
    public const int MIN_POINTS = 8; // Minimum points on a line for it to be considered a closed loop
    public float thickness = 0.1f; // How wide the line will be drawn

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

        // Add circle collider component for this point
        CircleCollider2D circleCollider = gameObject.AddComponent<CircleCollider2D>();
        circleCollider.offset = transform.InverseTransformPoint(position);
        circleCollider.radius = thickness / 2;
        colliders.Add(circleCollider);

        // Add line renderer position for this point
        lineRenderer.positionCount++;
        lineRenderer.SetPosition(lineRenderer.positionCount-1, transform.InverseTransformPoint(position));

        // Deduct doodle fuel if there's more than one point on this line
        if (lineRenderer.positionCount > 1) PlayerController.instance.DrawDoodleFuel(1);

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

        // Then check for minimum distance between points with local space-transformed position
        return Vector2.Distance(lineRenderer.GetPosition(lineRenderer.positionCount-1), transform.InverseTransformPoint(position)) > DrawManager.RESOLUTION;
    }
    public int GetPointsCount()
    {
        return lineRenderer.positionCount;
    }
    public Vector2 GetFirstPoint()
    {
        return (Vector2)lineRenderer.GetPosition(0);
    }
    public Vector2 GetLastPoint()
    {
        return (Vector2)lineRenderer.GetPosition(GetPointsCount() - 1);
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
}
