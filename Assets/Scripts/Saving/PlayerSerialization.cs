using System;
using UnityEngine;

[Serializable]
public class PlayerSerialization
{
    public Vector3Serialization spawnpoint;
    public Inventory inventory;
    public ToolType currentTool;

    public PlayerSerialization(PlayerVars player)
    {
        spawnpoint = new Vector3Serialization(player.GetSpawnPos());
        inventory = player.lastSavedInventory;
        currentTool = player.cur_tool;
    }

    public void SetValues(GameObject playerObj)
    {
        PlayerVars player = playerObj.GetComponent<PlayerVars>();
        player.lastSavedInventory.copy(inventory);
        player.inventory.copy(inventory);
        player.SetSpawnPos(spawnpoint.GetValue());
        if (player.inventory.hasTool(currentTool))
            DrawManager.instance.SwitchTool(currentTool);
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