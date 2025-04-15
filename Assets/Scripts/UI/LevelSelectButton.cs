using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelSelectButton : MonoBehaviour, ISelectHandler
{
    [SerializeField] private Sprite normal, selected;
    [SerializeField] private Button button;
    [SerializeField] private Image image;

    private void Awake()
    {
        SetButtonActive(false);
    }

    public void SetButtonActive(bool active)
    {
        image.sprite = active ? selected : normal;
        button.interactable = active;
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (button.interactable)
            button.onClick?.Invoke();
    }
}