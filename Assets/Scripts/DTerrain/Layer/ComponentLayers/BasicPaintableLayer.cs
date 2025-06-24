using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DTerrain
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class BasicPaintableLayer : PaintableLayer<PaintableChunk>
    {
        private SpriteRenderer spriteRenderer;
        private Quaternion rotation;
        private Vector3 scale;

        public void OnEnable()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            OriginalSprite = spriteRenderer.sprite;
            SortingLayer = spriteRenderer.sortingLayerID;
            SortingOrder = spriteRenderer.sortingOrder;
        }

        //CHUNK SIZE X!!!!
        public virtual void Start()
        {
            spriteRenderer.enabled = false;
            EraserFunctions.PaintableLayers.Add(this);
            rotation = transform.rotation;
            scale = transform.localScale;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            SpawnChunks();
            InitChunks();
            transform.rotation = rotation;
            transform.localScale = scale;
        }

        public virtual void Update()
        {
        }

        public virtual void OnDestroy()
        {
            EraserFunctions.PaintableLayers.Remove(this);
        }

        public void Paint(Vector3 pos, int circleSize)
        {
            Vector3 p = transform.InverseTransformPoint(new Vector3(pos.x, pos.y, 0)) + (Vector3)OriginalSprite.pivot / OriginalSprite.pixelsPerUnit;
            Shape destroyCircle = Shape.GenerateShapeOval((int)(circleSize / scale.x), (int)(circleSize / scale.y));

            Paint(new PaintingParameters()
            {
                Color = Color.clear,
                Position = new Vector2Int((int)(p.x * OriginalSprite.pixelsPerUnit) - (int)(circleSize / scale.x), (int)(p.y * OriginalSprite.pixelsPerUnit) - (int)(circleSize / scale.y)),
                Shape = destroyCircle,
                PaintingMode = PaintingMode.REPLACE_COLOR,
                DestructionMode = DestructionMode.DESTROY
            });
        }
    }
}
