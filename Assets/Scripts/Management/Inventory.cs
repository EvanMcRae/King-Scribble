using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory
{
    private Dictionary<ToolType, bool> toolUnlocks;
    public Inventory() // By default, the player starts with no tools
    {
        toolUnlocks = new Dictionary<ToolType, bool>();
        toolUnlocks.Add(ToolType.Pencil, false);
        toolUnlocks.Add(ToolType.Pen, false);
        toolUnlocks.Add(ToolType.Eraser, false);
    }
    public bool hasTool(ToolType tool) // Checks if the player has a given tool
    {
        if (toolUnlocks.ContainsKey(tool)) return toolUnlocks[tool];
        else return false;
    }
    public void addTool(ToolType tool) // Adds a given tool to the player's tool inventory
    {
        if (toolUnlocks.ContainsKey(tool)) toolUnlocks[tool] = true;
    }
    public void copy(Inventory other)
    {
        toolUnlocks = other.toolUnlocks;
    }
}
