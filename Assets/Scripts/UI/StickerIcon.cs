using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class StickerIcon : MonoBehaviour
{
    public static StickerIcon instance;
    private Image image;
    [SerializeField] private Sprite collected;

    void Awake()
    {
        instance = this;
        image = GetComponent<Image>();
        image.enabled = false;
    }
    public void Collect()
    {
        image.enabled = true;
        image.sprite = collected;
    }
    public void Show()
    {
        image.enabled = true;
    }
}
