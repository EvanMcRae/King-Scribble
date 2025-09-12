using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

[RequireComponent(typeof(SpriteMask))]
[RequireComponent(typeof(ParentConstraint))]
public class FlowMask : MonoBehaviour
{
    [SerializeField] private InteractableWater water;
    private SpriteMask spriteMask;
    private Sprite sprite;
    public Vector2Int fudgeFactor = new(5, 3);
    private bool queueComputeGeometry = false;

    void Awake()
    {
        spriteMask = GetComponent<SpriteMask>();
        var texture2D = new Texture2D((int)water.Width + fudgeFactor.x, (int)water.Height + fudgeFactor.y);
        sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), 0.5f * Vector2.one, 1);
        spriteMask.sprite = sprite;
        water.MaskUpdateAction += QueueComputeGeometry;
        GetComponent<ParentConstraint>().SetTranslationOffset(0, new Vector3(fudgeFactor.x / 2.0f, fudgeFactor.y / 2.0f));
    }

    void QueueComputeGeometry()
    {
        queueComputeGeometry = true;
    }

    void Update()
    {
        if (queueComputeGeometry)
        {
            ComputeGeometry();
            queueComputeGeometry = false;
        }
    }

    void ComputeGeometry()
    {
        Mesh mesh = water.WaterMesh;

        // Compute mesh bounds in 2D to normalize into Sprite.rect
        Bounds bounds = mesh.bounds;
        Vector2 meshMin = new(bounds.min.x, bounds.min.y);
        Vector2 meshSize = new(bounds.size.x, bounds.size.y);
        Vector2 spriteSize = sprite.rect.size;

        Vector2[] vertices = Utils.ConvertArray(mesh.vertices); ;

        for (int i = 0; i < mesh.vertexCount; i++)
        {
            // Normalize vertex within mesh bounds (0 to 1)
            Vector2 normalized = new(
                (vertices[i].x - meshMin.x) / meshSize.x,
                (vertices[i].y - meshMin.y) / meshSize.y
            );

            // Scale to sprite rect size in pixels
            vertices[i] = new(
                normalized.x * ((int)water.Width),
                normalized.y * ((int)water.Height)
            );
        }

        ushort[] triangles = new ushort[mesh.triangles.Length];
        for (int i = 0; i < mesh.triangles.Length; i++)
        {
            triangles[i] = (ushort)mesh.triangles[i];
        }
        sprite.OverrideGeometry(vertices, triangles);
    }
}
