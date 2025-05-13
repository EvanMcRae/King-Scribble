using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Referenced: https://www.youtube.com/watch?v=SmAwege_im8
public class Line : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private ParticleSystem penObjDestroy;
    public const float MASS_COEFF = 2f;
    public const float MAX_WEIGHT = 100f;
    public List<CircleCollider2D> colliders = new(); // TODO use this for eraser checking?
    public bool canDraw = true, hasDrawn = false;
    public const float LOOP_ALLOWANCE = 0.2f; // Maximum distance between the first and last point of a line to be considered a closed loop
    public const int MIN_POINTS = 8; // Minimum points on a line for it to be considered a closed loop
    public const float MAX_DISTANCE = 50f;
    public float thickness = 0.1f; // How wide the line will be drawn
    public bool collisionsActive = true; // If collisions are active while drawing (for pen - initially false, set to true on finish)
    public bool is_pen = false;
    public bool hasOverlapped = false;
    public bool deleted = false;
    public float area = 0;
    public SpriteRenderer startPoint;

    Vector2[] ConvertArray(Vector3[] v3){
        Vector2 [] v2 = new Vector2[v3.Length];
        for(int i = 0; i <  v3.Length; i++){
            v2[i] = v3[i];
        }
        return v2;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (rigidBody != null)
            rigidBody.isKinematic = true;

        lineRenderer.widthMultiplier = thickness;
        startPoint.transform.localScale += 2 * thickness * Vector3.one;
    }

    public void SetPosition(Vector2 position, bool forced = false, bool addFuel = true)
    {
        if (!forced && !CanAppend(position)) return;

        position = transform.InverseTransformPoint(position);

        // Special case to activate first collider
        if (lineRenderer.positionCount >= 1 && collisionsActive && !colliders[0].enabled)
            colliders[0].enabled = true;

        // If this point is too far away, march along it and add extra points
        if (!forced && lineRenderer.positionCount > 0 && Vector2.Distance(GetLastPoint(), position) > DrawManager.RESOLUTION)
        {
            Vector2 marchPos = GetLastPoint();
            do
            {
                marchPos = Vector2.MoveTowards(marchPos, position, DrawManager.RESOLUTION);
                AppendPos(marchPos, addFuel);
            } while (Vector2.Distance(marchPos, position) > DrawManager.RESOLUTION);
        }

        AppendPos(position, addFuel);

        hasDrawn = true;
    }

    private void AppendPos(Vector2 position, bool addFuel = true)
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
        if (lineRenderer.positionCount > 1 && addFuel)
        {
            int cost = lineRenderer.positionCount == 2 ? 2 : 1; // accounts for missing the first point
            if (PlayerVars.instance.cur_tool == ToolType.Pencil) PlayerVars.instance.SpendDoodleFuel(cost);
            else if (PlayerVars.instance.cur_tool == ToolType.Pen) PlayerVars.instance.SpendTempPenFuel(cost);
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
        float distance = Vector2.Distance(GetLastPoint(), transform.InverseTransformPoint(position));
        if (distance > MAX_DISTANCE) DrawManager.instance.EndDraw();
        return distance > DrawManager.RESOLUTION && distance < MAX_DISTANCE;
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
        return GetPointsCount() >= MIN_POINTS && Vector2.Distance(GetFirstPoint(), GetLastPoint()) <= (LOOP_ALLOWANCE + thickness);
    }

    public void AddPhysics()
    {
        gameObject.layer = 7; // Pen line layer
        gameObject.tag = "Pen";
        lineRenderer.SetPosition(GetPointsCount()-1, GetFirstPoint());
        // Apply physics behavior
        GetComponent<Rigidbody2D>().isKinematic = false;
        // Subtract pen fuel for each point in the finished object (since this will always be called when a pen object is finished drawing)
        PlayerVars.instance.SpendPenFuel(lineRenderer.positionCount);
        // Set weight based on area
        Vector3[] points = new Vector3[GetPointsCount()]; 
        lineRenderer.GetPositions(points); // Get an array containing all points in the line
        // Note: This is ugly. I know this is ugly. It works. (from https://stackoverflow.com/questions/2034540/calculating-area-of-irregular-polygon-in-c-sharp)
        area = Mathf.Abs(points.Take(GetPointsCount() - 1).Select((p, i) => (points[i + 1].x - p.x) * (points[i + 1].y + p.y)).Sum() / 2);
        rigidBody.mass = area * MASS_COEFF;
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
    public bool AddPolyCollider() // Return false if a collision overlap is found, true otherwise
    {
        // Get the list of points in the lineRenderer and convert to Vector2
        Vector3[] points3 = new Vector3[GetPointsCount()];
        lineRenderer.GetPositions(points3);
        Vector2[] points2 = ConvertArray(points3);
        // Create a polygon collider and set its path to the Vector2 list of points
        PolygonCollider2D polyCollider = gameObject.AddComponent<PolygonCollider2D>();
        polyCollider.SetPath(0, points2);
        List<Collider2D> results = new();
        ContactFilter2D def = new()
        {
            useLayerMask = true,
            layerMask =~ LayerMask.GetMask("NoDraw-Pencil") // Ignore collisions with the NoDraw-Pencil layer
        };
        if (polyCollider.OverlapCollider(def, results) != 0)
        {
            // Attempt to filter for colliders that could actually do this
            foreach (var result in results)
            {
                if ((1 << 7 & result.excludeLayers) != 0) // If PenLines layer is excluded
                {
                    continue;
                }
                deleted = true;
                Destroy(gameObject);
                return false;
            }
        }
        return true;
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
        startPoint.enabled = false;
    }

    void OnDestroy()
    {
        // Create the pen object destruction particle effect
        if (gameObject.scene.isLoaded) { // Only call if the destruction is not a result of a scene change/exit
            if (!gameObject.GetComponent<PolygonCollider2D>() || !gameObject.GetComponentInChildren<MeshFilter>()) return; // Only run on pen objects
            ParticleSystem part = Instantiate(penObjDestroy, gameObject.GetComponent<PolygonCollider2D>().bounds.center, Quaternion.identity);
            // Set the mesh of the particle system to the mesh of the pen object
            var shape = part.shape;
            shape.mesh = gameObject.GetComponentInChildren<MeshFilter>().mesh;
            
            if(area != 0) {
                // Set the number of particles based on the object's area
                var burst = part.emission.GetBurst(0);
                burst.count = 20 * area;
                part.emission.SetBurst(0, burst);
                // Play the effect (and then destroy)
                part.Play();
            } 
        }
    }

    public void SmoothPencil(int severity = 1) // Smooth the collider positions of a pencil line, to prevent janky player movement/collision
    {
        Vector3[] points3 = new Vector3[GetPointsCount()];
        lineRenderer.GetPositions(points3);
        Vector2[] points2 = ConvertArray(points3);

        for (int i = 1; i < GetPointsCount(); i++)
        {
            // Compute the average position of closest N neighbors, where N is the severity
            int startPos = i - severity > 0 ? i - severity : 0;
            int endPos = i + severity < GetPointsCount() ? i + severity : GetPointsCount();
            float avgX = 0f, avgY = 0f;
            for (int j = startPos; j < endPos; j++)
            {
                avgX += points2[j].x;
                avgY += points2[j].y;
            }
            avgX /= endPos - startPos;
            avgY /= endPos - startPos;
            // Set the collider's position to the average
            colliders[i].offset = new Vector2(avgX, avgY);
        }
    }

    public void SmoothPen(int severity = 1)
    {
        PolygonCollider2D polyCollider = gameObject.GetComponent<PolygonCollider2D>();
        if (!polyCollider) return;

        Vector2[] path = polyCollider.GetPath(0);

        for (int i = 1; i < path.Length; i++)
        {
            int startPos = i - severity > 0 ? i - severity : 0;
            int endPos = i + severity < path.Length ? i + severity : path.Length;
            float avgX = 0f, avgY = 0f;
            for (int j = startPos; j < endPos; j++)
            {
                avgX += path[j].x;
                avgY += path[j].y;
            }
            avgX /= endPos - startPos;
            avgY /= endPos - startPos;
            path[i] = new Vector2(avgX, avgY);
        }
        polyCollider.SetPath(0, path);
    }

    public void Generalize(float threshold = 0.1f, int minSize = 50) // Generalize a pen object - reducing its vertices by removing any that are closer than the given threshold
    {
        PolygonCollider2D polyCollider = gameObject.GetComponent<PolygonCollider2D>();
        if (!polyCollider) return;
        int numRemoved = 1;
        int iter = 0;
        while (numRemoved != 0 && GetPointsCount() > minSize)
        {
            iter ++;
            numRemoved = 0;
            List<Vector2> path = new(polyCollider.GetPath(0));
            for (int i = 1; i < path.Count - 1; i++)
            {
                if (Vector2.Distance(path[i - 1], path[i]) < threshold || Vector2.Distance(path[i], path[i + 1]) < threshold)
                {
                    path.RemoveAt(i);
                    numRemoved ++;
                }  
            }
            polyCollider.SetPath(0, path);
        }
        Debug.Log("Number of iterations for vertex culling: " + iter);
    }
}

