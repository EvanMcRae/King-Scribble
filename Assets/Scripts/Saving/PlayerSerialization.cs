using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerSerialization
{
    public Inventory inventory;
    public ToolType currentTool;

    public PlayerSerialization()
    {
        inventory = new();
        currentTool = ToolType.None;
    }

    public PlayerSerialization(PlayerVars player)
    {
        inventory = PlayerVars.lastSavedInventory;
        currentTool = player.last_tool;
    }

    public void SetValues(GameObject playerObj)
    {
        PlayerVars player = playerObj.GetComponent<PlayerVars>();
        PlayerVars.lastSavedInventory.copy(inventory);
        player.inventory.copy(inventory);
        if (player.inventory.hasTool(currentTool))
            player.cur_tool = currentTool;
    }
}

[Serializable]
public class Vector3Serialization
{
    public float x, y, z;

    public Vector3Serialization(Vector3 position)
    {
        x = position.x;
        y = position.y;
        z = position.z;
    }

    public Vector3 GetValue()
    {
        return new Vector3(x, y, z);
    }
}

[Serializable]
public class SceneSerialization
{
    // TODO extend with other scene data
    public string name;
    public Vector3Serialization spawnpoint;
    public List<string> unlockPoints;

    public SceneSerialization(string sceneName, Vector3 spawnPos)
    {
        name = sceneName;
        spawnpoint = new Vector3Serialization(spawnPos);
    }
}