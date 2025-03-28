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
    public bool hasOverlapped = false;

    Vector2[] ConvertArray(Vector3[] v3){
        Vector2 [] v2 = new Vector2[v3.Length];
        for(int i = 0; i <  v3.Length; i++){
            Vector3 tempV3 = v3[i];
            v2[i] = new Vector2(tempV3.x, tempV3.y);
        }
        return v2;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (rigidBody != null)
            rigidBody.isKinematic = true;

        lineRenderer.widthMultiplier = thickness;
    }

    public void SetPosition(Vector2 position, bool forced = false)
    {
        if (!forced && !CanAppend(position)) return;

        position = transform.InverseTransformPoint(position);

        // Special case to activate first collider
        if (lineRenderer.positionCount >= 1 && collisionsActive && !colliders[0].enabled)
            colliders[0].enabled = true;

        // If this point is too far away, march along it and add extra points
        if (lineRenderer.positionCount > 0 && Vector2.Distance(GetLastPoint(), position) > DrawManager.RESOLUTION)
        {
            Debug.Log("marching");
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
        // Add circle collider component for this point if using pencil
        if (!is_pen) {
            CircleCollider2D circleCollider = gameObject.AddComponent<CircleCollider2D>();
            circleCollider.offset = position;
            circleCollider.radius = thickness / 2;
            if (!collisionsActive || lineRenderer.positionCount == 0) circleCollider.enabled = false;
            colliders.Add(circleCollider);
        }
        // Add line renderer position for this point
        lineRenderer.positionCount++;
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, position);
        if (is_pen)
            CheckOverlap();

        // Deduct doodle fuel if there's more than one point on this line and using pencil
        if (lineRenderer.positionCount > 1)
        {
            if (PlayerVars.instance.cur_tool == ToolType.Pencil) PlayerVars.instance.SpendDoodleFuel(1);
            else if (PlayerVars.instance.cur_tool == ToolType.Pen) PlayerVars.instance.SpendTempPenFuel(1);
        }
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
        lineRenderer.SetPosition(GetPointsCount()-1, GetFirstPoint());
        // Apply physics behavior
        GetComponent<Rigidbody2D>().isKinematic = false;
        // Subtract pen fuel for each point in the finished object (since this will always be called when a pen object is finished drawing)
        PlayerVars.instance.SpendPenFuel(lineRenderer.positionCount);
        // Set weight based on area
        Vector3[] points = new Vector3[GetPointsCount()]; 
        lineRenderer.GetPositions(points); // Get an array containing all points in the line
        // Note: This is ugly. I know this is ugly. It works. (from https://stackoverflow.com/questions/2034540/calculating-area-of-irregular-polygon-in-c-sharp)
        var area = Mathf.Abs(points.Take(GetPointsCount() - 1).Select((p, i) => (points[i + 1].x - p.x) * (points[i + 1].y + p.y)).Sum() / 2);
        rigidBody.mass = area;
    }

    public void CheckOverlap()
    // Check if the most recently drawn point is within some small distance from any other point (aka, if the user has created a loop - closed or otherwise)
    {
        if (hasOverlapped) return;
        for (int i = 0; i < GetPointsCount() - 2; i++) // 2 instead of 1 to prevent detecting a collision with the previously drawn point (which would otherwise always happen)
        {
            if (Vector2.Distance(lineRenderer.GetPosition(i), GetLastPoint()) < 0.08f)
            {
                hasOverlapped = true;
                return;
            }
        }
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
    // Set the color of the line
    public void SetColor(Color color)
    {
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }
    // Create a polygon collider from the line renderer's points
    public void AddPolyCollider()
    {
        // Get the list of points in the lineRenderer and convert to Vector2
        Vector3[] points3 = new Vector3[GetPointsCount()];
        lineRenderer.GetPositions(points3);
        Vector2[] points2 = ConvertArray(points3);
        // Create a polygon collider and set its path to the Vector2 list of points
        PolygonCollider2D polyCollider = gameObject.AddComponent<PolygonCollider2D>();
        polyCollider.SetPath(0, points2);
    }
    // Add a mesh from the polygon collider (if it has been created)
    public void AddMesh(Material mat, MaterialPropertyBlock matBlock)
    {
        PolygonCollider2D polyCollider = gameObject.GetComponent<PolygonCollider2D>();
        if (!polyCollider) return; // Return if there is no polygon collider
        // Create the mesh, mesh renderer, and mesh filter
        Mesh polyMesh = polyCollider.CreateMesh(false, false); // (false, false allows its position to follow the rigidbody)
        MeshRenderer polyRend = GetComponentInChildren<MeshRenderer>();
        MeshFilter polyFilter = GetComponentInChildren<MeshFilter>();
        // Set the material, mesh and layer parameters
        polyRend.material = mat;
        polyRend.sortingLayerName = "Ground";
        polyRend.sortingOrder = -3;
        polyRend.SetPropertyBlock(matBlock);

        // Apply UV coordinates for texture rendering
        Vector2[] uvs = new Vector2[polyMesh.vertexCount];
        Bounds bounds = polyMesh.bounds;
        for (int i = 0; i < polyMesh.vertexCount; i++)
        {
            // Map each vertex to a UV based on its position relative to the bounds
            uvs[i] = new Vector2((polyMesh.vertices[i].x - bounds.min.x) / bounds.size.x, (polyMesh.vertices[i].y - bounds.min.y) / bounds.size.y);
        }
        polyMesh.uv = uvs;
        polyFilter.mesh = polyMesh;
    }
}

