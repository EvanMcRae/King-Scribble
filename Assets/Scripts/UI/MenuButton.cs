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
    public bool noSound = false;
    public static bool globalNoSound = false;
    private bool hovered = false;

    public void OnDeselect(BaseEventData eventData)
    {
        if (normal != null)
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
            hovered = true;
            HUDButtonCursorHandler handler = GetComponent<HUDButtonCursorHandler>();
            if (handler != null && handler.disablesWhileDrawing && DrawManager.instance != null && DrawManager.instance.IsUsingTool())
                return;
            if (!soundOnPointerEnter)
                noSound = true;
            EventSystem.current.SetSelectedGameObject(gameObject);
            noSound = false;
        }
    }

    public void Update()
    {
        HUDButtonCursorHandler handler = GetComponent<HUDButtonCursorHandler>();
        if (handler != null && handler.disablesWhileDrawing && (DrawManager.instance == null || !DrawManager.instance.IsUsingTool()) && GetComponent<Selectable>().interactable)
        {
            if (hovered && popupID == PopupPanel.numPopups)
            {
                if (!soundOnPointerEnter)
                    noSound = true;
                EventSystem.current.SetSelectedGameObject(gameObject);
                noSound = false;
            }
        }
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (eventData.hovered.Contains(gameObject) && EventSystem.current.currentSelectedGameObject != gameObject && popupID == PopupPanel.numPopups)
        {
            HUDButtonCursorHandler handler = GetComponent<HUDButtonCursorHandler>();
            if (handler != null && handler.disablesWhileDrawing && DrawManager.instance != null && DrawManager.instance.IsUsingTool())
                return;
            if (!soundOnPointerEnter)
                noSound = true;
            EventSystem.current.SetSelectedGameObject(gameObject);
            noSound = false;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (deselectsOnPointerLeave && EventSystem.current.currentSelectedGameObject == gameObject)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        hovered = false;
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (selected != null)
            image.sprite = selected;

        // The many conditions for not playing the select sound :,)
        if (!noSound && !globalNoSound && !PopupPanel.closedThisFrame
            && ((mainMenu && MainMenuManager.firstopen) || !mainMenu)
            && ((pauseMenu && PauseMenu.firstopen) || !pauseMenu)
            && ((image != null && image.enabled) || image == null))
        {
            soundPlayer.PlaySound(select);
        }
    }

    public void OnClick()
    {
        if (!globalNoSound && !noSound)
            soundPlayer.PlaySound(press);
    }
}