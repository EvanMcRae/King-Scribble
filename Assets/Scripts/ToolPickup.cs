using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolPickup : Collectible
{
    public ToolType type;
    public override void OnPickup(Collider2D player)
    {
        // Add the tool to the player's inventory 
        player.gameObject.transform.root.GetComponent<PlayerVars>().inventory.addTool(type);
    }
}
