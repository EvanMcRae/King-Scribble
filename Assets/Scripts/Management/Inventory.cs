using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Inventory
{
    public List<ToolType> _toolTypes;

    public Inventory() // By default, the player starts with no tools
    {
        _toolTypes = new List<ToolType>();
    }

    public bool hasTool(ToolType tool) // Checks if the player has a given tool
    {
        return _toolTypes.Contains(tool);
    }

    public void addTool(ToolType toolType) // Adds a given tool to the player's tool inventory
    {
        if (!_toolTypes.Contains(toolType))
        {
            _toolTypes.Add(toolType);
        }
    }
    
    public void copy(Inventory other)
    {
        _toolTypes = new List<ToolType>(other._toolTypes);
    }
}
