using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HUDButtonCursorHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static bool inside = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        DrawManager.instance.SetCursor(ToolType.None);
        inside = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!GameManager.paused && !GameManager.resetting)
        {
            if (PlayerVars.instance != null)
                DrawManager.instance.SetCursor(PlayerVars.instance.cur_tool);
        }
        inside = false;
    }
}
