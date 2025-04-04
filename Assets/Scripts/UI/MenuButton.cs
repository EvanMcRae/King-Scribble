using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler, IDeselectHandler, ISelectHandler
{
    [SerializeField] private Sprite normal, selected;
    [SerializeField] private bool MainMenu;
    [SerializeField] private Image image;
    public int popupID = 0;
    public bool deselectsOnPointerLeave = false;

    public void OnDeselect(BaseEventData eventData)
    {
        if (normal != null && PopupPanel.numPopups == 0)
            image.sprite = normal;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (PopupPanel.mouseNeverMoved > 0)
        {
            if (!deselectsOnPointerLeave)
            {
                PopupPanel.mouseNeverMoved--;
                return;
            }
            else
            {
                PopupPanel.mouseNeverMoved = 0;
            }
        }

        if (popupID == PopupPanel.numPopups)
            EventSystem.current.SetSelectedGameObject(gameObject);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (eventData.hovered.Contains(gameObject) && EventSystem.current.currentSelectedGameObject != gameObject && popupID == PopupPanel.numPopups)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (deselectsOnPointerLeave && EventSystem.current.currentSelectedGameObject == gameObject)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (selected != null)
            image.sprite = selected;
    }
}