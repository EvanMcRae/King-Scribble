using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory
{
    private Dictionary<string, bool> toolUnlocks;
    public Inventory() // By default, the player only starts with the pencil
    {
        toolUnlocks = new Dictionary<string, bool>();
        toolUnlocks.Add("Pencil", true);
        toolUnlocks.Add("Pen", false);
        toolUnlocks.Add("Eraser", false);
    }
    public bool hasTool(string tool) // Checks if the player has a given tool
    {
        if (toolUnlocks.ContainsKey(tool)) return toolUnlocks[tool];
        else return false;
    }
    public void addTool(string tool) // Adds a given tool to the player's tool inventory
    {
        if (toolUnlocks.ContainsKey(tool)) toolUnlocks[tool] = true;
        // Debug.Log("Tool " + tool + " added.");
    }
}
