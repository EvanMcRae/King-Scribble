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
            SpawnChunks();
            InitChunks();
        }

        public virtual void Update()
        {
        }

        public virtual void OnDestroy()
        {
            EraserFunctions.PaintableLayers.Remove(this);
        }
    }
}
