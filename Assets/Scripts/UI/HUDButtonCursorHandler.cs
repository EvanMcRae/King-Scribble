using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HUDButtonCursorHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static bool inside = false;
    public bool disablesWhileDrawing = false;
    private bool hovering = false;


    public void Update()
    {
        if (!GameManager.paused && !GameManager.resetting && hovering)
        {
            if (!DrawManager.instance.isDrawing && disablesWhileDrawing && !inside)
            {
                GetComponent<Button>().interactable = true;
                inside = true;
                DrawManager.instance.SetCursor(ToolType.None);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (disablesWhileDrawing && DrawManager.instance.isDrawing)
        {
            GetComponent<Button>().interactable = false;
        }
        else
        {
            inside = true;
            DrawManager.instance.SetCursor(ToolType.None);
        }
        hovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!GameManager.paused && !GameManager.resetting)
        {
            if (PlayerVars.instance != null)
                DrawManager.instance.SetCursor(PlayerVars.instance.cur_tool);
            if (disablesWhileDrawing && DrawManager.instance.isDrawing)
            {
                GetComponent<Button>().interactable = true;
            }
        }
        inside = false;
        hovering = false;
    }

    // TODO: When releasing draw and still inside, set inside to true which will then disable drawing
}
