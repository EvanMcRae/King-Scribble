using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory
{
    private Dictionary<string, bool> toolUnlocks;
    public Inventory()
    {
        toolUnlocks = new Dictionary<string, bool>();
        toolUnlocks.Add("Pencil", true);
        toolUnlocks.Add("Pen", false);
        toolUnlocks.Add("Eraser", false);
    }
    public bool hasTool(string tool)
    {
        if (toolUnlocks.ContainsKey(tool)) return toolUnlocks[tool];
        else return false;
    }
    public void addTool(string tool)
    {
        if (toolUnlocks.ContainsKey(tool)) toolUnlocks[tool] = true;
        // Debug.Log("Tool " + tool + " added.");
    }
}
