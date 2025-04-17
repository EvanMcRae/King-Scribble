using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolIndicator : MonoBehaviour
{
    public Image PencilIcon, PenIcon, EraserIcon, PCIcon;
    public Sprite PencilUnused, PencilUsed, PenUnused, PenUsed, EraserUnused, EraserUsed;
    public List<Sprite> PencilCaseSprites = new();
    public List<Image> Slots = new();
    public static ToolIndicator instance;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        UpdateMenu(PlayerVars.instance.cur_tool);
    }

    public void UpdateMenu(ToolType tool = ToolType.None)
    {
        foreach (Image slot in Slots)
        {
            slot.gameObject.SetActive(false);
        }

        for (int i = 0; i < PlayerVars.instance.inventory.toolUnlocks.Count; i++)
        {
            ToolType currTool = PlayerVars.instance.inventory.toolUnlocks[i];

            // TODO this feels really bad but interim solutions :,)
            Sprite used = null, unused = null;
            switch (currTool)
            {
                case ToolType.Pencil:
                    used = PencilUsed;
                    unused = PencilUnused;
                    break;
                case ToolType.Pen:
                    used = PenUsed;
                    unused = PenUnused;
                    break;
                case ToolType.Eraser:
                    used = EraserUsed;
                    unused = EraserUnused;
                    break;
            }

            // Set slot sprite
            Debug.Log(tool + " " + currTool);
            Slots[i].sprite = currTool == tool ? used : unused;
            Slots[i].gameObject.SetActive(true);
        }

        PCIcon.sprite = PencilCaseSprites[PlayerVars.instance.inventory.toolUnlocks.Count];
    }
}
