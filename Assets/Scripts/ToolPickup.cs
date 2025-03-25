using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolPickup : Collectible
{
    public ToolType type;
    public override void OnPickup(Collider2D player)
    {
        // Add the tool to the player's inventory 
        // Note: System.Enum.GetName(typeof(ToolType), type) converts the enum from int into its string name in the enum definition (i.e. 1 = "Pencil")
        player.gameObject.transform.root.GetComponent<PlayerVars>().inventory.addTool(System.Enum.GetName(typeof(ToolType), type));
    }
}
