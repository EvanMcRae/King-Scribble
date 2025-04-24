using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class LevelSelectButton : MonoBehaviour, ISelectHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Sprite off, hover, on;
    [SerializeField] private Button button;
    [SerializeField] private Image image;
    private bool bouncing = false;
    [SerializeField] private SoundPlayer soundPlayer;

    private void Awake()
    {
        SetButtonActive(false);
    }

    public void SetButtonActive(bool active)
    {
        image.sprite = active ? on : off;
        button.interactable = active;
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (button.interactable)
            button.onClick?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button.interactable)
            StartBouncing();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (button.interactable)
            StopBouncing();
    }

    public void StartBouncing()
    {
        bouncing = true;
        soundPlayer.PlaySound("UI.Select");
        BounceUp();
    }

    public void StopBouncing()
    {
        bouncing = false;
    }

    void BounceUp()
    {
        transform.DOScale(1.2f, 0.35f).OnComplete(() => { BounceDown(); });
    }

    void BounceDown()
    {
        transform.DOScale(1f, 0.35f).OnComplete(() => { if (bouncing) BounceUp(); });
    }
}