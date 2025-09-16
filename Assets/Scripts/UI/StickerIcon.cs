using DG.Tweening;
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
    public void Collect(bool animate = false)
    {
        image.enabled = true;
        image.sprite = collected;
        if (animate)
        {
            transform.DOScale(1.2f, 0.1f).OnComplete(() =>
        {
            transform.DOScale(1.0f, 0.1f);
        });
        }
    }
    public void Show()
    {
        image.enabled = true;
    }
}
