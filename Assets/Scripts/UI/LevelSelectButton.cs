using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelSelectButton : MonoBehaviour, ISelectHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Sprite off, hover, on;
    [SerializeField] private Button button;
    [SerializeField] private Image image;

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
            image.sprite = hover;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (button.interactable)
            image.sprite = on;
    }
}