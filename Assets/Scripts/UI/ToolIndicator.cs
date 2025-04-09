using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolIndicator : MonoBehaviour
{
    public Image PencilIcon, PenIcon, EraserIcon;
    public Sprite PencilUnused, PencilUsed, PenUnused, PenUsed, EraserUnused, EraserUsed;

    public static ToolIndicator instance;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        UpdateMenu();
    }

    // TODO: This is really poorly coded, sorry, interim solutions :,)
    public void UpdateMenu(ToolType tool = ToolType.None)
    {
        if (tool != ToolType.None) // For pickups to not edit selected item
        {
            PencilIcon.sprite = tool == ToolType.Pencil ? PencilUsed : PencilUnused;
            PenIcon.sprite = tool == ToolType.Pen ? PenUsed : PenUnused;
            EraserIcon.sprite = tool == ToolType.Eraser ? EraserUsed : EraserUnused;
        }

        PencilIcon.gameObject.SetActive(PlayerVars.instance.inventory.hasTool(ToolType.Pencil));
        PenIcon.gameObject.SetActive(PlayerVars.instance.inventory.hasTool(ToolType.Pen));
        EraserIcon.gameObject.SetActive(PlayerVars.instance.inventory.hasTool(ToolType.Eraser));
    }
}
