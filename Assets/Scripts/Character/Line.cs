using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Referenced: https://www.youtube.com/watch?v=SmAwege_im8
public class Line : MonoBehaviour
{
    [SerializeField] private GameObject linePoint;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private EdgeCollider2D cldr;
    private readonly List<Vector2> points = new List<Vector2>();
    public bool canDraw = true, hasDrawn = false;

    // Start is called before the first frame update
    void Start()
    {
        cldr.transform.position = Vector3.zero;
        if (GetComponent<Rigidbody2D>() != null)
            GetComponent<Rigidbody2D>().isKinematic = true;
    }

    public void SetPosition(Vector2 position)
    {
        if (!CanAppend(position)) return;

        if (lineRenderer.positionCount > 0) PlayerController.instance.DrawDoodleFuel(1);

        // TODO DEBUG circle colliders
        CircleCollider2D circleCollider = gameObject.AddComponent<CircleCollider2D>();
        circleCollider.offset = transform.InverseTransformPoint(position);
        circleCollider.radius = 0.1f;

        points.Add(position);
        lineRenderer.positionCount++;
        lineRenderer.SetPosition(lineRenderer.positionCount-1, transform.InverseTransformPoint(position));
        cldr.points = points.ToArray();

        hasDrawn = true;
    }

    private bool CanAppend(Vector2 position)
    {
        position = transform.InverseTransformPoint(position);

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
