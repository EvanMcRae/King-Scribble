using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler, IDeselectHandler, ISelectHandler
{
    [SerializeField] private Sprite normal, selected;

    [SerializeField] private bool mainMenu;
    [SerializeField] private bool pauseMenu;
    [SerializeField] private Image image;
    public int popupID = 0;
    public bool deselectsOnPointerLeave = false;
    [SerializeField] private SoundPlayer soundPlayer;
    [SerializeField] private SoundClip select, press;
    public bool soundOnPointerEnter = true;
    private bool noSound = false;

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
        {
            if (!soundOnPointerEnter)
                noSound = true;
            EventSystem.current.SetSelectedGameObject(gameObject);
            noSound = false;
        }
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
        if (!noSound && ((mainMenu && MainMenuManager.firstopen) || !mainMenu) && ((pauseMenu && PauseMenu.firstopen) || !pauseMenu) && !PopupPanel.closedThisFrame)
            soundPlayer.PlaySound(select);
    }

    public void OnClick()
    {
        soundPlayer.PlaySound(press);
    }
}