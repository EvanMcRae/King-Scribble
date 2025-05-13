using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StickerBoard : MonoBehaviour
{
    [SerializeField] private List<Image> stickers;
    private Color disabledColor = new(0, 0, 0, 0.5f);

    // Start is called before the first frame update
    void OnEnable()
    {
        for (int i = 0; i < stickers.Count; i++)
        {
            if (GameSaver.currData.stickers.Contains((Sticker.StickerType)i))
            {
                stickers[i].color = Color.white;
            }
            else
            {
                stickers[i].color = disabledColor;
            }
        }
    }
}
