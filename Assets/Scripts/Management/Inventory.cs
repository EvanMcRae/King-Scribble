using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory
{
    private List<ToolType> toolUnlocks;
    public Inventory() // By default, the player starts with no tools
    {
        toolUnlocks = new List<ToolType>();
    }
    public bool hasTool(ToolType tool) // Checks if the player has a given tool
    {
        return toolUnlocks.Contains(tool);
    }
    public void addTool(ToolType tool) // Adds a given tool to the player's tool inventory
    {
        toolUnlocks.Add(tool);
    }
    public void copy(Inventory other)
    {
        toolUnlocks = new List<ToolType>(other.toolUnlocks);
    }
}
