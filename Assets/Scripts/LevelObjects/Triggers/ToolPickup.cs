using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolPickup : Collectible
{
    public ToolType _toolType;
    public override void OnPickup(Collider2D player)
    {
        // Add the tool to the player's inventory 
        PlayerVars.instance.inventory.addTool(_toolType);
        PlayerController.instance.CollectTool();
        // Switch to the tool
        DrawManager.instance.TrySwitchTool(_toolType);
    }
}
