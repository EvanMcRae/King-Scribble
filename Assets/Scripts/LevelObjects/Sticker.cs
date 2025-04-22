using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sticker : Collectible
{
    public StickerType type;
    private bool deleted = false;

    public void Start()
    {
        if (GameSaver.currData.stickers.Contains(type))
        {
            deleted = true;
            Destroy(gameObject);
        }
    }
    public override void OnPickup(Collider2D player)
    {
        if (!deleted)
            GameSaver.currData.stickers.Add(type);
    }

    public enum StickerType
    {
        DINO,
        APPLE,
        BASKETBALL,
        STAR,
        RIBBON,
        LLL,
        BANANA
    }
}
