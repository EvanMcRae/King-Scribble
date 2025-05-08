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
    [SerializeField] private GameObject disableOrigin;

    public void Update()
    {
        if (!GameManager.paused && !GameManager.resetting && hovering)
        {
            if (!DrawManager.instance.IsUsingTool() && disablesWhileDrawing && !inside)
            {
                if (disableOrigin == null)
                    GetComponent<Button>().interactable = true;
                else foreach (Button b in disableOrigin.GetComponentsInChildren<Button>())
                {
                    b.interactable = true;
                }
                inside = true;
                DrawManager.instance.SetCursor(ToolType.None);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (disablesWhileDrawing && DrawManager.instance.IsUsingTool())
        {
            if (disableOrigin == null)
                GetComponent<Button>().interactable = false;
            else foreach (Button b in disableOrigin.GetComponentsInChildren<Button>())
            {
                b.interactable = false;
            }
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
            if (disablesWhileDrawing && DrawManager.instance.IsUsingTool())
            {
                if (disableOrigin == null)
                    GetComponent<Button>().interactable = true;
                else foreach (Button b in disableOrigin.GetComponentsInChildren<Button>())
                {
                    b.interactable = true;
                }
            }
        }
        inside = false;
        hovering = false;
    }
}
