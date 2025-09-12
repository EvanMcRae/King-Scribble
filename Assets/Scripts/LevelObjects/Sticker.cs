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
            StickerIcon.instance.Collect();
        }
        else
        {
            StickerIcon.instance.Show();
        }
    }
    public override void OnPickup(Collider2D player)
    {
        if (!deleted)
        {
            GameSaver.tempStickers.Add(type);
            PlayerController.instance.CollectSticker();
            StickerIcon.instance.Collect();
        }
    }

    public enum StickerType
    {
        STAR,
        BASKETBALL,
        BANANA,
        APPLE,
        DINO,
        RIBBON
    }
}
